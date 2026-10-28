using CodeWalker.GameFiles;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeWalker.Tools
{
    public enum MloIssueSeverity
    {
        Info,
        Warning,
        Error,
    }

    //One finding from the MLO doctor. All geometry is in the MLO's local space (the 3D view transforms it
    //by the placed interior's position and rotation). Highlight shows the problem, Preview shows what a fix would change.
    public class MloIssue
    {
        public MloIssueSeverity Severity;
        public string Check;
        public string Message;
        public string Cause;
        public string FixDescription;
        public Action Fix;
        public List<Vector3> Highlight = new List<Vector3>(); //line segment pairs
        public List<Vector3> Preview = new List<Vector3>(); //line segment pairs
        public bool HasFocus;
        public Vector3 Focus;

        public bool Fixable { get { return Fix != null; } }

        public override string ToString()
        {
            return Severity + ": " + Message;
        }
    }

    public class MloVisibilityResult
    {
        public int StartRoom = -1;
        public Dictionary<int, int> Hops = new Dictionary<int, int>(); //rooms connected by portals, with how many portals away
        public HashSet<int> Visible = new HashSet<int>(); //rooms seen through a chain of portals from the eye position
        public HashSet<int> TraversedPortals = new HashSet<int>();
        public bool OutsideVisible;
        public bool Truncated;
    }

    public static class MloDoctor
    {
        private const float BoundsPad = 0.5f;


        //---------------------------------------------------------------------------------------------------------
        // geometry helpers

        public static void AddBox(List<Vector3> segs, Vector3 min, Vector3 max)
        {
            var c = new Vector3[8];
            for (int i = 0; i < 8; i++)
            {
                c[i] = new Vector3((i & 1) != 0 ? max.X : min.X, (i & 2) != 0 ? max.Y : min.Y, (i & 4) != 0 ? max.Z : min.Z);
            }
            int[] e = { 0, 1, 2, 3, 4, 5, 6, 7, 0, 2, 1, 3, 4, 6, 5, 7, 0, 4, 1, 5, 2, 6, 3, 7 };
            for (int i = 0; i < e.Length; i += 2)
            {
                segs.Add(c[e[i]]);
                segs.Add(c[e[i + 1]]);
            }
        }
        public static void AddPoly(List<Vector3> segs, IList<Vector3> pts)
        {
            for (int i = 0; i < pts.Count; i++)
            {
                segs.Add(pts[i]);
                segs.Add(pts[(i + 1) % pts.Count]);
            }
        }
        public static void AddCross(List<Vector3> segs, Vector3 p, float s)
        {
            segs.Add(p - Vector3.UnitX * s); segs.Add(p + Vector3.UnitX * s);
            segs.Add(p - Vector3.UnitY * s); segs.Add(p + Vector3.UnitY * s);
            segs.Add(p - Vector3.UnitZ * s); segs.Add(p + Vector3.UnitZ * s);
        }

        private static Vector3[] CornerPoints(MCMloPortalDef p)
        {
            if (p?.Corners == null) return new Vector3[0];
            return p.Corners.Select(c => new Vector3(c.X, c.Y, c.Z)).ToArray();
        }

        private static bool ValidBounds(MCMloRoomDef r)
        {
            var a = r._Data.bbMin; var b = r._Data.bbMax;
            return (a.X <= b.X) && (a.Y <= b.Y) && (a.Z <= b.Z);
        }
        private static bool InBounds(MCMloRoomDef r, Vector3 p, float pad)
        {
            var a = r._Data.bbMin; var b = r._Data.bbMax;
            return (p.X >= a.X - pad) && (p.X <= b.X + pad) && (p.Y >= a.Y - pad) && (p.Y <= b.Y + pad) && (p.Z >= a.Z - pad) && (p.Z <= b.Z + pad);
        }
        private static float DistToBounds(MCMloRoomDef r, Vector3 p)
        {
            var a = r._Data.bbMin; var b = r._Data.bbMax;
            var d = Vector3.Max(Vector3.Max(a - p, p - b), Vector3.Zero);
            return d.Length();
        }
        private static float Volume(MCMloRoomDef r)
        {
            var s = r._Data.bbMax - r._Data.bbMin;
            return Math.Max(s.X, 0) * Math.Max(s.Y, 0) * Math.Max(s.Z, 0);
        }
        private static string RoomLabel(MloArchetype mlo, int i)
        {
            if ((mlo.rooms == null) || (i < 0) || (i >= mlo.rooms.Length)) return "room " + i;
            var n = mlo.rooms[i].RoomName;
            return string.IsNullOrEmpty(n) ? "room " + i : "'" + n + "' (" + i + ")";
        }

        //Best-fit plane normal of a polygon, even when it is a self-crossing "bow-tie" whose area cancels out
        private static Vector3 PolyNormal(Vector3[] pts)
        {
            Vector3 n = Vector3.Zero;
            for (int i = 0; i < pts.Length; i++)
            {
                var a = pts[i]; var b = pts[(i + 1) % pts.Length];
                n.X += (a.Y - b.Y) * (a.Z + b.Z);
                n.Y += (a.Z - b.Z) * (a.X + b.X);
                n.Z += (a.X - b.X) * (a.Y + b.Y);
            }
            if (n.Length() > 1e-4f) return n;
            Vector3 best = Vector3.Zero;
            for (int i = 0; i < pts.Length; i++)
                for (int j = i + 1; j < pts.Length; j++)
                    for (int k = j + 1; k < pts.Length; k++)
                    {
                        var c = Vector3.Cross(pts[j] - pts[i], pts[k] - pts[i]);
                        if (c.LengthSquared() > best.LengthSquared()) best = c;
                    }
            return best;
        }

        private static bool SegmentsCross2D(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            Func<Vector2, Vector2, Vector2, float> orient = (a, b, c) => (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
            float d1 = orient(p3, p4, p1), d2 = orient(p3, p4, p2), d3 = orient(p1, p2, p3), d4 = orient(p1, p2, p4);
            return (((d1 > 0) && (d2 < 0)) || ((d1 < 0) && (d2 > 0))) && (((d3 > 0) && (d4 < 0)) || ((d3 < 0) && (d4 > 0)));
        }

        //true when two non-neighbouring edges of the polygon cross each other (corners listed in a twisted order)
        private static bool SelfCrossing(Vector3[] pts, Vector3 n)
        {
            if (pts.Length < 4) return false;
            n.Normalize();
            var u = Vector3.Normalize(Math.Abs(n.Z) < 0.9f ? Vector3.Cross(n, Vector3.UnitZ) : Vector3.Cross(n, Vector3.UnitX));
            var v = Vector3.Cross(n, u);
            var p2 = pts.Select(p => new Vector2(Vector3.Dot(p, u), Vector3.Dot(p, v))).ToArray();
            for (int i = 0; i < p2.Length; i++)
            {
                for (int j = i + 1; j < p2.Length; j++)
                {
                    if ((j == i + 1) || ((i == 0) && (j == p2.Length - 1))) continue; //neighbours share a corner
                    if (SegmentsCross2D(p2[i], p2[(i + 1) % p2.Length], p2[j], p2[(j + 1) % p2.Length])) return true;
                }
            }
            return false;
        }

        //Re-sorts corners around their centre so the outline no longer twists, facing the same way as normalDir
        private static Vector3[] UntwistCorners(Vector3[] pts, Vector3 normalDir, out int[] order)
        {
            var n = Vector3.Normalize(normalDir);
            var centre = pts.Aggregate(Vector3.Zero, (a, b) => a + b) / pts.Length;
            var u = Vector3.Normalize(pts[0] - centre - n * Vector3.Dot(pts[0] - centre, n));
            var v = Vector3.Cross(n, u);
            order = Enumerable.Range(0, pts.Length).OrderBy(i =>
            {
                var d = pts[i] - centre;
                return Math.Atan2(Vector3.Dot(d, v), Vector3.Dot(d, u));
            }).ToArray();
            return order.Select(i => pts[i]).ToArray();
        }


        //---------------------------------------------------------------------------------------------------------
        // analysis

        public static List<MloIssue> Analyze(MloArchetype mlo, GameFileCache gfc)
        {
            var issues = new List<MloIssue>();
            if (mlo == null) return issues;
            try
            {
                CheckRooms(mlo, issues);
                CheckPortals(mlo, issues);
                CheckGraph(mlo, issues);
                CheckEntities(mlo, gfc, issues);
                CheckCollision(mlo, gfc, issues);
            }
            catch (Exception ex)
            {
                issues.Add(new MloIssue { Severity = MloIssueSeverity.Warning, Check = "Doctor", Message = "The check stopped early because of unexpected data: " + ex.Message, Cause = "The MLO contains values the doctor didn't expect." });
            }
            return issues.OrderByDescending(i => i.Severity).ThenBy(i => i.Check).ToList();
        }

        private static void CheckRooms(MloArchetype mlo, List<MloIssue> issues)
        {
            var rooms = mlo.rooms;
            if ((rooms == null) || (rooms.Length == 0))
            {
                issues.Add(new MloIssue { Severity = MloIssueSeverity.Error, Check = "Rooms", Message = "This MLO has no rooms.", Cause = "An interior needs at least the 'limbo' room (0) plus one real room." });
                return;
            }
            if ((rooms[0].RoomName ?? "").ToLowerInvariant() != "limbo")
            {
                issues.Add(new MloIssue { Severity = MloIssueSeverity.Info, Check = "Rooms", Message = "Room 0 is called " + RoomLabel(mlo, 0) + ", not 'limbo'.", Cause = "Room 0 is the 'outside' room that portals to the world connect to. Custom names work, but check it's meant to be the exterior." });
            }
            var portals = mlo.portals ?? new MCMloPortalDef[0];
            for (int i = 0; i < rooms.Length; i++)
            {
                var r = rooms[i];
                if (i > 0)
                {
                    if (!ValidBounds(r))
                    {
                        var iss = new MloIssue { Severity = MloIssueSeverity.Error, Check = "Rooms", Message = RoomLabel(mlo, i) + " has bounds where min is bigger than max.", Cause = "The room box is inside-out, so the game can never find you inside this room." };
                        var a = r._Data.bbMin; var b = r._Data.bbMax;
                        iss.Fix = () => { r._Data = With(r._Data, Vector3.Min(a, b), Vector3.Max(a, b)); };
                        iss.FixDescription = "Swap min and max so the box is the right way round";
                        AddBox(iss.Highlight, Vector3.Min(a, b), Vector3.Max(a, b));
                        AddBox(iss.Preview, Vector3.Min(a, b), Vector3.Max(a, b));
                        iss.HasFocus = true; iss.Focus = (a + b) * 0.5f;
                        issues.Add(iss);
                    }
                    else if (Volume(r) < 1e-3f)
                    {
                        var iss = new MloIssue { Severity = MloIssueSeverity.Warning, Check = "Rooms", Message = RoomLabel(mlo, i) + " has almost no volume.", Cause = "A flat or empty room box can't contain the player or camera." };
                        iss.HasFocus = true; iss.Focus = (r._Data.bbMin + r._Data.bbMax) * 0.5f;
                        AddCross(iss.Highlight, iss.Focus, 0.5f);
                        issues.Add(iss);
                    }
                    if ((r.AttachedObjects == null) || (r.AttachedObjects.Length == 0))
                    {
                        var iss = new MloIssue { Severity = MloIssueSeverity.Warning, Check = "Rooms", Message = RoomLabel(mlo, i) + " has no entities attached.", Cause = "Nothing is assigned to this room, so nothing is drawn when you are inside it." };
                        if (ValidBounds(r)) { AddBox(iss.Highlight, r._Data.bbMin, r._Data.bbMax); iss.HasFocus = true; iss.Focus = r.BBCenter; }
                        issues.Add(iss);
                    }
                }

                uint count = 0;
                foreach (var p in portals) if ((p._Data.roomFrom == i) || (p._Data.roomTo == i)) count++;
                if (count != r._Data.portalCount)
                {
                    var iss = new MloIssue { Severity = MloIssueSeverity.Warning, Check = "Rooms", Message = RoomLabel(mlo, i) + " says it has " + r._Data.portalCount + " portals but " + count + " use it.", Cause = "The stored portal count is out of date (usually after editing portals by hand)." };
                    iss.FixDescription = "Recalculate the portal count of every room";
                    iss.Fix = () => mlo.UpdatePortalCounts();
                    issues.Add(iss);
                }
            }

            //overlapping bounds
            for (int i = 1; i < rooms.Length; i++)
            {
                for (int j = i + 1; j < rooms.Length; j++)
                {
                    if (!ValidBounds(rooms[i]) || !ValidBounds(rooms[j])) continue;
                    var mn = Vector3.Max(rooms[i]._Data.bbMin, rooms[j]._Data.bbMin);
                    var mx = Vector3.Min(rooms[i]._Data.bbMax, rooms[j]._Data.bbMax);
                    var s = mx - mn;
                    if ((s.X <= 0) || (s.Y <= 0) || (s.Z <= 0)) continue;
                    float ov = s.X * s.Y * s.Z;
                    float small = Math.Min(Volume(rooms[i]), Volume(rooms[j]));
                    if (small <= 1e-4f) continue;
                    float frac = ov / small;
                    if (frac < 0.4f) continue;
                    var iss = new MloIssue { Severity = MloIssueSeverity.Warning, Check = "Overlap", Message = RoomLabel(mlo, i) + " and " + RoomLabel(mlo, j) + " overlap by " + (int)(frac * 100) + "% of the smaller room.", Cause = "When rooms share space the game can pick the wrong room for the player or camera, causing wrong lighting or parts of the interior disappearing." };
                    AddBox(iss.Highlight, rooms[i]._Data.bbMin, rooms[i]._Data.bbMax);
                    AddBox(iss.Highlight, rooms[j]._Data.bbMin, rooms[j]._Data.bbMax);
                    iss.HasFocus = true; iss.Focus = (mn + mx) * 0.5f;
                    issues.Add(iss);
                }
            }
        }

        private static CMloRoomDef With(CMloRoomDef d, Vector3 min, Vector3 max)
        {
            d.bbMin = min;
            d.bbMax = max;
            return d;
        }

        private static void CheckPortals(MloArchetype mlo, List<MloIssue> issues)
        {
            var portals = mlo.portals;
            if (portals == null) return;
            var rooms = mlo.rooms ?? new MCMloRoomDef[0];
            int ents = mlo.entities?.Length ?? 0;

            var votes = new List<KeyValuePair<int, int>>(); //portal index, orientation sign
            for (int i = 0; i < portals.Length; i++)
            {
                var p = portals[i];
                int pi = i;
                var pts = CornerPoints(p);
                string label = "Portal " + i + " (" + RoomLabel(mlo, (int)p._Data.roomFrom) + " to " + RoomLabel(mlo, (int)p._Data.roomTo) + ")";
                bool geomOk = pts.Length >= 3;

                if (!geomOk)
                {
                    issues.Add(new MloIssue { Severity = MloIssueSeverity.Error, Check = "Portals", Message = label + " has fewer than 3 corners.", Cause = "A portal is a flat polygon and needs at least three corners (normally four)." });
                }
                else
                {
                    AddPortalGeometryChecks(mlo, issues, p, pi, pts, label);
                }

                if ((p._Data.roomFrom >= rooms.Length) || (p._Data.roomTo >= rooms.Length))
                {
                    var iss = new MloIssue { Severity = MloIssueSeverity.Error, Check = "References", Message = label + " points at a room that doesn't exist (the MLO has " + rooms.Length + " rooms).", Cause = "Rooms were removed or reordered after the portal was made." };
                    if (geomOk) { AddPoly(iss.Highlight, pts); iss.HasFocus = true; iss.Focus = p.Center; }
                    issues.Add(iss);
                }
                else if (p._Data.roomFrom == p._Data.roomTo)
                {
                    var iss = new MloIssue { Severity = MloIssueSeverity.Error, Check = "Portals", Message = label + " connects a room to itself.", Cause = "Both sides of the portal use the same room number, so it links nothing." };
                    if (geomOk) { AddPoly(iss.Highlight, pts); iss.HasFocus = true; iss.Focus = p.Center; }
                    issues.Add(iss);
                }
                else if (geomOk)
                {
                    //does the portal actually sit between the two rooms?
                    foreach (var ri in new[] { (int)p._Data.roomFrom, (int)p._Data.roomTo })
                    {
                        if ((ri == 0) || !ValidBounds(rooms[ri])) continue;
                        float d = pts.Min(c => DistToBounds(rooms[ri], c));
                        float dc = DistToBounds(rooms[ri], p.Center);
                        if (Math.Min(d, dc) > 1.0f)
                        {
                            var iss = new MloIssue { Severity = MloIssueSeverity.Warning, Check = "Portals", Message = label + " is " + d.ToString("0.0") + " m away from " + RoomLabel(mlo, ri) + ".", Cause = "The portal doesn't touch one of the rooms it connects, so the game may never see through it." };
                            AddPoly(iss.Highlight, pts);
                            AddBox(iss.Highlight, rooms[ri]._Data.bbMin, rooms[ri]._Data.bbMax);
                            iss.HasFocus = true; iss.Focus = p.Center;
                            issues.Add(iss);
                        }
                    }
                    if ((p._Data.roomFrom > 0) && (p._Data.roomTo > 0) && ValidBounds(rooms[p._Data.roomFrom]) && ValidBounds(rooms[p._Data.roomTo]))
                    {
                        var n = PolyNormal(pts);
                        var d = rooms[p._Data.roomTo].BBCenter - rooms[p._Data.roomFrom].BBCenter;
                        float dot = Vector3.Dot(Vector3.Normalize(n), Vector3.Normalize(d));
                        if (Math.Abs(dot) > 0.2f) votes.Add(new KeyValuePair<int, int>(i, dot > 0 ? 1 : -1));
                    }
                }

                if (p.AttachedObjects != null)
                {
                    foreach (var idx in p.AttachedObjects)
                    {
                        if (idx >= ents)
                        {
                            uint bad = idx;
                            var iss = new MloIssue { Severity = MloIssueSeverity.Error, Check = "References", Message = label + " has an attached entity #" + idx + " but the MLO only has " + ents + " entities.", Cause = "An entity was deleted without removing it from the portal." };
                            iss.FixDescription = "Remove the missing entity from the portal";
                            iss.Fix = () => { p.AttachedObjects = p.AttachedObjects.Where(x => x != bad).ToArray(); };
                            if (geomOk) { AddPoly(iss.Highlight, pts); iss.HasFocus = true; iss.Focus = p.Center; }
                            issues.Add(iss);
                        }
                    }
                }
            }

            //orientation: portals between real rooms should all face the same way relative to from->to
            if (votes.Count >= 3)
            {
                int plus = votes.Count(v => v.Value > 0);
                int minus = votes.Count - plus;
                int majority = (plus >= minus) ? 1 : -1;
                if (Math.Min(plus, minus) > 0 && Math.Min(plus, minus) < Math.Max(plus, minus))
                {
                    foreach (var v in votes.Where(v => v.Value != majority))
                    {
                        var p = portals[v.Key];
                        var pts = CornerPoints(p);
                        var iss = new MloIssue { Severity = MloIssueSeverity.Warning, Check = "Orientation", Message = "Portal " + v.Key + " faces the opposite way to most portals (" + Math.Max(plus, minus) + " of " + votes.Count + " agree).", Cause = "Its corner winding runs the other way compared with the rest, so the game may treat its 'from' and 'to' sides swapped. Confirm in game: this is a likely cause, not a certainty." };
                        iss.FixDescription = "Reverse the corner order (keeps the same rooms)";
                        iss.Fix = () => { p.Corners = p.Corners.Reverse().ToArray(); };
                        AddPoly(iss.Highlight, pts);
                        AddPoly(iss.Preview, pts.Reverse().ToArray());
                        AddNormalArrow(iss.Preview, pts.Reverse().ToArray());
                        AddNormalArrow(iss.Highlight, pts);
                        iss.HasFocus = true; iss.Focus = p.Center;
                        issues.Add(iss);
                    }
                }
            }

            //duplicates
            for (int i = 0; i < portals.Length; i++)
            {
                for (int j = i + 1; j < portals.Length; j++)
                {
                    var a = portals[i]; var b = portals[j];
                    bool same = ((a._Data.roomFrom == b._Data.roomFrom) && (a._Data.roomTo == b._Data.roomTo)) || ((a._Data.roomFrom == b._Data.roomTo) && (a._Data.roomTo == b._Data.roomFrom));
                    if (same && ((a.Center - b.Center).Length() < 0.25f))
                    {
                        var iss = new MloIssue { Severity = MloIssueSeverity.Info, Check = "Portals", Message = "Portals " + i + " and " + j + " are in the same place and join the same rooms.", Cause = "One of them is probably a leftover duplicate." };
                        AddPoly(iss.Highlight, CornerPoints(a));
                        iss.HasFocus = true; iss.Focus = a.Center;
                        issues.Add(iss);
                    }
                }
            }
        }

        private static void AddNormalArrow(List<Vector3> segs, Vector3[] pts)
        {
            if (pts.Length < 3) return;
            var n = PolyNormal(pts);
            if (n.LengthSquared() < 1e-8f) return;
            n.Normalize();
            var c = pts.Aggregate(Vector3.Zero, (a, b) => a + b) / pts.Length;
            var tip = c + n * 1.0f;
            segs.Add(c); segs.Add(tip);
            var side = Vector3.Cross(n, Math.Abs(n.Z) < 0.9f ? Vector3.UnitZ : Vector3.UnitX);
            side.Normalize();
            segs.Add(tip); segs.Add(tip - n * 0.25f + side * 0.12f);
            segs.Add(tip); segs.Add(tip - n * 0.25f - side * 0.12f);
        }

        private static void AddPortalGeometryChecks(MloArchetype mlo, List<MloIssue> issues, MCMloPortalDef p, int pi, Vector3[] pts, string label)
        {
            var n = PolyNormal(pts);
            if (n.LengthSquared() < 1e-8f)
            {
                var iss = new MloIssue { Severity = MloIssueSeverity.Error, Check = "Portals", Message = label + " has all corners on one line or point.", Cause = "The corners are collinear or identical, so the portal has no area." };
                iss.HasFocus = true; iss.Focus = p.Center; AddCross(iss.Highlight, p.Center, 0.5f);
                issues.Add(iss);
                return;
            }

            //repeated corners
            var dedup = new List<int>();
            for (int i = 0; i < pts.Length; i++)
            {
                var prev = pts[(i + pts.Length - 1) % pts.Length];
                if ((pts[i] - prev).Length() > 1e-3f) dedup.Add(i);
            }
            if ((dedup.Count != pts.Length) && (dedup.Count >= 3))
            {
                var keep = dedup.ToArray();
                var iss = new MloIssue { Severity = MloIssueSeverity.Warning, Check = "Portals", Message = label + " repeats " + (pts.Length - dedup.Count) + " corner(s).", Cause = "Two neighbouring corners sit in the same place, giving the portal a zero-length edge." };
                iss.FixDescription = "Remove the repeated corners";
                iss.Fix = () => { p.Corners = keep.Select(k => p.Corners[k]).ToArray(); };
                AddPoly(iss.Highlight, pts);
                AddPoly(iss.Preview, keep.Select(k => pts[k]).ToArray());
                iss.HasFocus = true; iss.Focus = p.Center;
                issues.Add(iss);
            }

            //twisted corner order
            if (SelfCrossing(pts, n))
            {
                var iss = new MloIssue { Severity = MloIssueSeverity.Error, Check = "Corner order", Message = label + " has its corners in a twisted order (the outline crosses itself).", Cause = "Corners were listed in the wrong sequence, giving a bow-tie shape. The game can't see through a crossed polygon properly, so the interior can vanish." };
                int[] order;
                var fixedPts = UntwistCorners(pts, n, out order);
                var ord = order;
                iss.FixDescription = "Re-sort the corners so the outline goes around once";
                iss.Fix = () => { var old = p.Corners; p.Corners = ord.Select(k => old[k]).ToArray(); };
                AddPoly(iss.Highlight, pts);
                AddPoly(iss.Preview, fixedPts);
                iss.HasFocus = true; iss.Focus = p.Center;
                issues.Add(iss);
            }

            //flatness
            var nn = Vector3.Normalize(n);
            var centre = pts.Aggregate(Vector3.Zero, (a, b) => a + b) / pts.Length;
            float dev = pts.Max(c => Math.Abs(Vector3.Dot(c - centre, nn)));
            if (dev > 0.05f)
            {
                var iss = new MloIssue { Severity = MloIssueSeverity.Warning, Check = "Portals", Message = label + " is not flat; corners are up to " + dev.ToString("0.00") + " m off its plane.", Cause = "Portals should be flat. A warped one can leak or clip in unexpected ways." };
                AddPoly(iss.Highlight, pts);
                if (dev < 0.5f)
                {
                    var flat = pts.Select(c => c - nn * Vector3.Dot(c - centre, nn)).ToArray();
                    iss.FixDescription = "Flatten the corners onto their average plane";
                    iss.Fix = () =>
                    {
                        var old = p.Corners;
                        p.Corners = old.Select((c, k) => new Vector4(flat[k], c.W)).ToArray();
                    };
                    AddPoly(iss.Preview, flat);
                }
                iss.HasFocus = true; iss.Focus = p.Center;
                issues.Add(iss);
            }
        }

        private static void CheckGraph(MloArchetype mlo, List<MloIssue> issues)
        {
            var rooms = mlo.rooms;
            if ((rooms == null) || (rooms.Length < 2)) return;
            var portals = mlo.portals ?? new MCMloPortalDef[0];
            var adj = BuildAdjacency(mlo);

            if (!portals.Any(p => (p._Data.roomFrom == 0) || (p._Data.roomTo == 0)))
            {
                issues.Add(new MloIssue { Severity = MloIssueSeverity.Error, Check = "Connectivity", Message = "No portal connects the interior to room 0 (outside).", Cause = "Without a portal to the exterior the interior can't be seen from the world or entered." });
            }

            var seen = new HashSet<int> { 0 };
            var queue = new Queue<int>();
            queue.Enqueue(0);
            while (queue.Count > 0)
            {
                int r = queue.Dequeue();
                foreach (var n in adj[r]) if (seen.Add(n)) queue.Enqueue(n);
            }
            for (int i = 1; i < rooms.Length; i++)
            {
                if (seen.Contains(i)) continue;
                var iss = new MloIssue { Severity = MloIssueSeverity.Error, Check = "Connectivity", Message = RoomLabel(mlo, i) + " can't be reached from outside through any chain of portals.", Cause = "No portal path leads from the exterior to this room (" + (adj[i].Count == 0 ? "it has no portals at all" : "its portals only lead to other cut-off rooms") + "), so it won't draw from outside or from other rooms." };
                if (ValidBounds(rooms[i])) { AddBox(iss.Highlight, rooms[i]._Data.bbMin, rooms[i]._Data.bbMax); iss.HasFocus = true; iss.Focus = rooms[i].BBCenter; }
                foreach (var p in portals.Where(p => (p._Data.roomFrom == i) || (p._Data.roomTo == i))) AddPoly(iss.Highlight, CornerPoints(p));
                issues.Add(iss);
            }
        }

        public static List<int>[] BuildAdjacency(MloArchetype mlo)
        {
            int rc = mlo.rooms?.Length ?? 0;
            var adj = new List<int>[rc];
            for (int i = 0; i < rc; i++) adj[i] = new List<int>();
            if (mlo.portals == null) return adj;
            foreach (var p in mlo.portals)
            {
                int a = (int)p._Data.roomFrom, b = (int)p._Data.roomTo;
                if ((a >= rc) || (b >= rc) || (a == b)) continue;
                if (!adj[a].Contains(b)) adj[a].Add(b);
                if (!adj[b].Contains(a)) adj[b].Add(a);
            }
            return adj;
        }

        private static void CheckEntities(MloArchetype mlo, GameFileCache gfc, List<MloIssue> issues)
        {
            var ents = mlo.entities ?? new MCEntityDef[0];
            var rooms = mlo.rooms ?? new MCMloRoomDef[0];
            var portals = mlo.portals ?? new MCMloPortalDef[0];
            var inRooms = new List<int>[ents.Length];
            var inPortals = new int[ents.Length];

            for (int r = 0; r < rooms.Length; r++)
            {
                if (rooms[r].AttachedObjects == null) continue;
                int ri = r;
                foreach (var idx in rooms[r].AttachedObjects)
                {
                    if (idx >= ents.Length)
                    {
                        uint bad = idx;
                        var iss = new MloIssue { Severity = MloIssueSeverity.Error, Check = "References", Message = RoomLabel(mlo, r) + " has an attached entity #" + idx + " but the MLO only has " + ents.Length + " entities.", Cause = "An entity was deleted or reordered without updating the room's list." };
                        iss.FixDescription = "Remove the missing entity from the room";
                        iss.Fix = () => { rooms[ri].AttachedObjects = rooms[ri].AttachedObjects.Where(x => x != bad).ToArray(); };
                        if (ValidBounds(rooms[r])) { AddBox(iss.Highlight, rooms[r]._Data.bbMin, rooms[r]._Data.bbMax); iss.HasFocus = true; iss.Focus = rooms[r].BBCenter; }
                        issues.Add(iss);
                        continue;
                    }
                    if (inRooms[idx] == null) inRooms[idx] = new List<int>();
                    inRooms[idx].Add(r);
                }
            }
            foreach (var p in portals)
            {
                if (p.AttachedObjects == null) continue;
                foreach (var idx in p.AttachedObjects) if (idx < ents.Length) inPortals[idx]++;
            }

            for (int e = 0; e < ents.Length; e++)
            {
                var ent = ents[e];
                string name = ent._Data.archetypeName.ToString();
                var pos = ent._Data.position;
                int ei = e;
                var rl = inRooms[e];

                if (((rl == null) || (rl.Count == 0)) && (inPortals[e] == 0))
                {
                    var iss = new MloIssue { Severity = MloIssueSeverity.Warning, Check = "Entities", Message = "Entity " + e + " (" + name + ") isn't attached to any room or portal.", Cause = "Entities that belong to no room don't draw as part of the interior." };
                    AddCross(iss.Highlight, pos, 0.4f);
                    iss.HasFocus = true; iss.Focus = pos;
                    var cand = Enumerable.Range(1, Math.Max(rooms.Length - 1, 0)).Where(r => ValidBounds(rooms[r]) && InBounds(rooms[r], pos, 0)).ToList();
                    if (cand.Count == 1)
                    {
                        int target = cand[0];
                        iss.FixDescription = "Attach it to " + RoomLabel(mlo, target) + ", the only room whose bounds contain it";
                        iss.Fix = () => { rooms[target].AttachedObjects = (rooms[target].AttachedObjects ?? new uint[0]).Concat(new[] { (uint)ei }).ToArray(); };
                        AddBox(iss.Preview, rooms[target]._Data.bbMin, rooms[target]._Data.bbMax);
                        iss.Preview.Add(pos); iss.Preview.Add(rooms[target].BBCenter);
                    }
                    issues.Add(iss);
                }
                else if ((rl != null) && (rl.Count > 1))
                {
                    var iss = new MloIssue { Severity = MloIssueSeverity.Warning, Check = "Entities", Message = "Entity " + e + " (" + name + ") is attached to " + rl.Count + " rooms: " + string.Join(", ", rl.Select(r => RoomLabel(mlo, r))) + ".", Cause = "An entity should belong to one room. Being in several can make it draw twice or pop in and out." };
                    AddCross(iss.Highlight, pos, 0.4f);
                    iss.HasFocus = true; iss.Focus = pos;
                    var containing = rl.Where(r => r > 0 && ValidBounds(rooms[r]) && InBounds(rooms[r], pos, 0)).ToList();
                    if (containing.Count == 1)
                    {
                        int keep = containing[0];
                        iss.FixDescription = "Keep it only in " + RoomLabel(mlo, keep) + " (the room whose bounds contain it)";
                        iss.Fix = () => { foreach (var r in rl.Where(r => r != keep)) rooms[r].AttachedObjects = rooms[r].AttachedObjects.Where(x => x != ei).ToArray(); };
                        AddBox(iss.Preview, rooms[keep]._Data.bbMin, rooms[keep]._Data.bbMax);
                    }
                    issues.Add(iss);
                }
                else if ((rl != null) && (rl.Count == 1) && (rl[0] > 0) && ValidBounds(rooms[rl[0]]) && !InBounds(rooms[rl[0]], pos, BoundsPad))
                {
                    int cur = rl[0];
                    var iss = new MloIssue { Severity = MloIssueSeverity.Warning, Check = "Entities", Message = "Entity " + e + " (" + name + ") is outside the bounds of its room " + RoomLabel(mlo, cur) + ".", Cause = "It may be assigned to the wrong room. (Entities near doorways can legitimately sit just outside.)" };
                    AddCross(iss.Highlight, pos, 0.4f);
                    AddBox(iss.Highlight, rooms[cur]._Data.bbMin, rooms[cur]._Data.bbMax);
                    iss.HasFocus = true; iss.Focus = pos;
                    var cand = Enumerable.Range(1, rooms.Length - 1).Where(r => r != cur && ValidBounds(rooms[r]) && InBounds(rooms[r], pos, 0)).ToList();
                    if (cand.Count == 1)
                    {
                        int target = cand[0];
                        iss.FixDescription = "Move it to " + RoomLabel(mlo, target) + ", the room whose bounds contain it";
                        iss.Fix = () =>
                        {
                            rooms[cur].AttachedObjects = rooms[cur].AttachedObjects.Where(x => x != ei).ToArray();
                            rooms[target].AttachedObjects = (rooms[target].AttachedObjects ?? new uint[0]).Concat(new[] { (uint)ei }).ToArray();
                        };
                        AddBox(iss.Preview, rooms[target]._Data.bbMin, rooms[target]._Data.bbMax);
                        iss.Preview.Add(pos); iss.Preview.Add(rooms[target].BBCenter);
                    }
                    issues.Add(iss);
                }

                if ((gfc != null) && (gfc.IsInited) && (gfc.GetArchetype(ent._Data.archetypeName) == null))
                {
                    var iss = new MloIssue { Severity = MloIssueSeverity.Info, Check = "References", Message = "Entity " + e + " uses model '" + name + "' which couldn't be found.", Cause = "Its ytyp/ydr isn't loaded or doesn't exist, so it will be invisible. (Some destruction models legitimately aren't found.)" };
                    AddCross(iss.Highlight, pos, 0.4f);
                    iss.HasFocus = true; iss.Focus = pos;
                    issues.Add(iss);
                }
            }

            if (mlo.entitySets != null)
            {
                foreach (var set in mlo.entitySets)
                {
                    int cnt = set.Entities?.Length ?? 0;
                    int locs = set.Locations?.Length ?? 0;
                    if (cnt != locs)
                    {
                        var s = set;
                        var iss = new MloIssue { Severity = MloIssueSeverity.Error, Check = "Entity sets", Message = "Entity set '" + set.Name + "' has " + cnt + " entities but " + locs + " room locations.", Cause = "Every entity in a set needs one location entry; a mismatch can misplace or drop entities." };
                        iss.FixDescription = "Pad or trim the locations so there is one per entity";
                        iss.Fix = () => { var l = (s.Locations ?? new uint[0]).Take(cnt).ToList(); while (l.Count < cnt) l.Add(0); s.Locations = l.ToArray(); };
                        issues.Add(iss);
                    }
                    else if (cnt == 0)
                    {
                        issues.Add(new MloIssue { Severity = MloIssueSeverity.Info, Check = "Entity sets", Message = "Entity set '" + set.Name + "' is empty.", Cause = "Nothing to show when this set is enabled." });
                    }
                }
            }
        }

        private static void CheckCollision(MloArchetype mlo, GameFileCache gfc, List<MloIssue> issues)
        {
            if ((gfc == null) || !gfc.IsInited) return;
            var rooms = mlo.rooms;
            if (rooms == null) return;
            YbnFile ybn = null;
            try { ybn = gfc.GetYbn(mlo.Hash.Hash); } catch { }
            if (ybn == null)
            {
                issues.Add(new MloIssue { Severity = MloIssueSeverity.Info, Check = "Collision", Message = "No collision (ybn) named '" + mlo.Name + "' was found for this interior.", Cause = "Players and peds may fall through the floor, unless collision comes from the props or isn't loaded yet." });
                return;
            }
            if (!ybn.Loaded || (ybn.Bounds == null))
            {
                issues.Add(new MloIssue { Severity = MloIssueSeverity.Info, Check = "Collision", Message = "The collision file is still loading; collision coverage wasn't checked. Re-scan in a moment.", Cause = "Game files load in the background." });
                return;
            }
            var bmin = ybn.Bounds.BoxMin; var bmax = ybn.Bounds.BoxMax;
            for (int i = 1; i < rooms.Length; i++)
            {
                if (!ValidBounds(rooms[i]) || (Volume(rooms[i]) < 1e-3f)) continue;
                var a = rooms[i]._Data.bbMin; var b = rooms[i]._Data.bbMax;
                const float tol = 1.5f;
                if ((a.X < bmin.X - tol) || (a.Y < bmin.Y - tol) || (a.Z < bmin.Z - tol) || (b.X > bmax.X + tol) || (b.Y > bmax.Y + tol) || (b.Z > bmax.Z + tol))
                {
                    var iss = new MloIssue { Severity = MloIssueSeverity.Warning, Check = "Collision", Message = RoomLabel(mlo, i) + " extends more than " + tol + " m beyond the interior's collision.", Cause = "Part of this room has no collision covering it, so you could fall out or walk through walls there." };
                    AddBox(iss.Highlight, a, b);
                    AddBox(iss.Highlight, bmin, bmax);
                    iss.HasFocus = true; iss.Focus = rooms[i].BBCenter;
                    issues.Add(iss);
                }
            }
        }


        //---------------------------------------------------------------------------------------------------------
        // visibility simulation

        public static int FindRoom(MloArchetype mlo, Vector3 local)
        {
            if (mlo?.rooms == null) return -1;
            int best = -1;
            float bestVol = float.MaxValue;
            for (int i = 1; i < mlo.rooms.Length; i++)
            {
                if (!ValidBounds(mlo.rooms[i]) || !InBounds(mlo.rooms[i], local, 0)) continue;
                float v = Volume(mlo.rooms[i]);
                if (v < bestVol) { bestVol = v; best = i; }
            }
            return best;
        }

        //Which rooms can be seen from a point in a room, by following the portal graph and clipping each portal
        //against the opening of the previous ones. Portals are treated as visible from both sides, so this is a
        //deliberately generous estimate; engine-specific behaviour should be confirmed in game.
        public static MloVisibilityResult SimulateVisibility(MloArchetype mlo, Vector3 eye, int startRoom, int maxDepth)
        {
            var res = new MloVisibilityResult { StartRoom = startRoom };
            if ((mlo?.rooms == null) || (startRoom < 0) || (startRoom >= mlo.rooms.Length)) return res;

            //plain graph distance
            var adj = BuildAdjacency(mlo);
            res.Hops[startRoom] = 0;
            var q = new Queue<int>();
            q.Enqueue(startRoom);
            while (q.Count > 0)
            {
                int r = q.Dequeue();
                foreach (var n in adj[r])
                {
                    if (res.Hops.ContainsKey(n)) continue;
                    res.Hops[n] = res.Hops[r] + 1;
                    q.Enqueue(n);
                }
            }

            //portal clipping
            int budget = 20000;
            var path = new HashSet<int>();
            Visit(mlo, eye, startRoom, new List<Plane>(), 0, maxDepth, path, res, ref budget);
            res.Visible.Add(startRoom);
            res.Truncated = budget <= 0;
            return res;
        }

        private static void Visit(MloArchetype mlo, Vector3 eye, int room, List<Plane> planes, int depth, int maxDepth, HashSet<int> path, MloVisibilityResult res, ref int budget)
        {
            if (depth >= maxDepth) return;
            for (int pi = 0; pi < mlo.portals.Length; pi++)
            {
                if (budget-- <= 0) return;
                var p = mlo.portals[pi];
                int from = (int)p._Data.roomFrom, to = (int)p._Data.roomTo;
                int other = (from == room) ? to : (to == room) ? from : -1;
                if ((other < 0) || (other == room) || (other >= mlo.rooms.Length)) continue;
                if (path.Contains(pi)) continue;
                var poly = CornerPoints(p).ToList();
                if (poly.Count < 3) continue;

                foreach (var pl in planes)
                {
                    poly = ClipPolygon(poly, pl);
                    if (poly.Count < 3) break;
                }
                if (poly.Count < 3) continue;

                res.TraversedPortals.Add(pi);
                if (other == 0)
                {
                    res.OutsideVisible = true;
                    continue; //don't walk into the open world
                }
                res.Visible.Add(other);

                var next = new List<Plane>();
                var centroid = poly.Aggregate(Vector3.Zero, (a, b) => a + b) / poly.Count;
                bool eyeOnPlane = Math.Abs(Vector3.Dot(Vector3.Normalize(PolyNormal(poly.ToArray())), eye - centroid)) < 0.02f;
                if (!eyeOnPlane)
                {
                    for (int i = 0; i < poly.Count; i++)
                    {
                        var a = poly[i] - eye; var b = poly[(i + 1) % poly.Count] - eye;
                        var n = Vector3.Cross(a, b);
                        if (n.LengthSquared() < 1e-10f) continue;
                        n.Normalize();
                        if (Vector3.Dot(n, centroid - eye) < 0) n = -n;
                        next.Add(new Plane(n, -Vector3.Dot(n, eye)));
                    }
                }
                else
                {
                    next.AddRange(planes);
                }
                path.Add(pi);
                Visit(mlo, eye, other, next, depth + 1, maxDepth, path, res, ref budget);
                path.Remove(pi);
            }
        }

        private static List<Vector3> ClipPolygon(List<Vector3> poly, Plane pl)
        {
            var o = new List<Vector3>();
            for (int i = 0; i < poly.Count; i++)
            {
                var a = poly[i]; var b = poly[(i + 1) % poly.Count];
                float da = Vector3.Dot(pl.Normal, a) + pl.D;
                float db = Vector3.Dot(pl.Normal, b) + pl.D;
                if (da >= 0) o.Add(a);
                if ((da >= 0) != (db >= 0))
                {
                    float t = da / (da - db);
                    o.Add(a + (b - a) * t);
                }
            }
            return o;
        }
    }
}
