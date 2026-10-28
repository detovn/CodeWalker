using CodeWalker.GameFiles;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeWalker.World
{
    public enum PlacementAlign
    {
        Min,
        Center,
        Max,
    }

    public class SmartPlacementItem
    {
        public YmapEntityDef Entity;
        public Vector3 OldPos;
        public Quaternion OldRot;
        public Vector3 NewPos;
        public Quaternion NewRot;

        public bool Changed
        {
            get
            {
                return ((NewPos - OldPos).LengthSquared() > 1e-8f) || (Quaternion.Dot(NewRot, OldRot) < 0.9999999f && Quaternion.Dot(NewRot, OldRot) > -0.9999999f);
            }
        }

        public void Bounds(out Vector3 min, out Vector3 max)
        {
            SmartPlacement.GetBounds(Entity, NewPos, NewRot, out min, out max);
        }
    }

    //Proposes new positions/rotations for a set of entities. Nothing is written to the entities until
    //Commit() is called by the caller, so the proposal can be previewed (and cancelled) first.
    public class SmartPlacement
    {
        public List<SmartPlacementItem> Items = new List<SmartPlacementItem>();
        public SmartPlacementItem Anchor; //the most recently selected entity, used as the reference for some commands

        public bool Changed
        {
            get { return Items.Any(i => i.Changed); }
        }

        public SmartPlacement(IEnumerable<YmapEntityDef> entities, YmapEntityDef anchor)
        {
            foreach (var e in entities)
            {
                if (e == null) continue;
                if (Items.Any(i => i.Entity == e)) continue;
                var item = new SmartPlacementItem();
                item.Entity = e;
                item.OldPos = item.NewPos = e.Position;
                item.OldRot = item.NewRot = e.Orientation;
                Items.Add(item);
                if (e == anchor) Anchor = item;
            }
            if ((Anchor == null) && (Items.Count > 0)) Anchor = Items[Items.Count - 1];
        }

        public void Reset()
        {
            foreach (var i in Items)
            {
                i.NewPos = i.OldPos;
                i.NewRot = i.OldRot;
            }
        }


        public static void GetBounds(YmapEntityDef e, Vector3 pos, Quaternion rot, out Vector3 min, out Vector3 max)
        {
            var arch = e.Archetype;
            Vector3 bmin = (arch != null) ? Vector3.Min(arch.BBMin, arch.BBMax) : new Vector3(-0.5f);
            Vector3 bmax = (arch != null) ? Vector3.Max(arch.BBMin, arch.BBMax) : new Vector3(0.5f);
            var mat = Matrix.Transformation(Vector3.Zero, Quaternion.Identity, e.Scale, Vector3.Zero, rot, pos);
            var matabs = mat;
            matabs.Column1 = mat.Column1.Abs();
            matabs.Column2 = mat.Column2.Abs();
            matabs.Column3 = mat.Column3.Abs();
            var centre = Vector3.TransformCoordinate((bmax + bmin) * 0.5f, mat);
            var extent = Vector3.TransformNormal((bmax - bmin) * 0.5f, matabs).Abs();
            min = centre - extent;
            max = centre + extent;
        }

        private void GroupBounds(out Vector3 min, out Vector3 max)
        {
            min = new Vector3(float.MaxValue);
            max = new Vector3(float.MinValue);
            foreach (var i in Items)
            {
                i.Bounds(out var bmin, out var bmax);
                min = Vector3.Min(min, bmin);
                max = Vector3.Max(max, bmax);
            }
        }

        private static float Get(Vector3 v, int axis)
        {
            return (axis == 0) ? v.X : (axis == 1) ? v.Y : v.Z;
        }
        private static Vector3 Offset(int axis, float d)
        {
            return new Vector3(axis == 0 ? d : 0, axis == 1 ? d : 0, axis == 2 ? d : 0);
        }

        public static float YawOf(Quaternion q)
        {
            var f = q.Multiply(Vector3.UnitY);
            return (float)Math.Atan2(-f.X, f.Y);
        }
        private static Quaternion YawQuat(float yaw)
        {
            return Quaternion.RotationAxis(Vector3.UnitZ, yaw);
        }


        //Align the bounds of every item on one axis (0=X, 1=Y, 2=Z). Aligns to the anchor's bounds if
        //toAnchor is set, otherwise to the bounds of the whole group.
        public void Align(int axis, PlacementAlign mode, bool toAnchor)
        {
            if (Items.Count < 2) return;
            Vector3 gmin, gmax;
            if (toAnchor && (Anchor != null)) Anchor.Bounds(out gmin, out gmax);
            else GroupBounds(out gmin, out gmax);
            float target = (mode == PlacementAlign.Min) ? Get(gmin, axis) : (mode == PlacementAlign.Max) ? Get(gmax, axis) : (Get(gmin, axis) + Get(gmax, axis)) * 0.5f;
            foreach (var i in Items)
            {
                if (toAnchor && (i == Anchor)) continue;
                i.Bounds(out var bmin, out var bmax);
                float cur = (mode == PlacementAlign.Min) ? Get(bmin, axis) : (mode == PlacementAlign.Max) ? Get(bmax, axis) : (Get(bmin, axis) + Get(bmax, axis)) * 0.5f;
                i.NewPos += Offset(axis, target - cur);
            }
        }

        //Space items evenly along an axis. With equalGaps the empty space between neighbouring bounds is
        //made equal, otherwise the centres are evenly spaced. The two outermost items stay put.
        public bool Distribute(int axis, bool equalGaps)
        {
            if (Items.Count < 3) return false;
            var sorted = Items.OrderBy(i => { i.Bounds(out var a, out var b); return Get(a, axis) + Get(b, axis); }).ToList();
            var first = sorted[0];
            var last = sorted[sorted.Count - 1];
            first.Bounds(out var fmin, out var fmax);
            last.Bounds(out var lmin, out var lmax);
            if (equalGaps)
            {
                float span = Get(lmax, axis) - Get(fmin, axis);
                float sizes = 0;
                foreach (var i in sorted)
                {
                    i.Bounds(out var a, out var b);
                    sizes += Get(b, axis) - Get(a, axis);
                }
                float gap = (span - sizes) / (sorted.Count - 1);
                float cursor = Get(fmin, axis);
                foreach (var i in sorted)
                {
                    i.Bounds(out var a, out var b);
                    float size = Get(b, axis) - Get(a, axis);
                    i.NewPos += Offset(axis, cursor - Get(a, axis));
                    cursor += size + gap;
                }
            }
            else
            {
                float c0 = (Get(fmin, axis) + Get(fmax, axis)) * 0.5f;
                float c1 = (Get(lmin, axis) + Get(lmax, axis)) * 0.5f;
                for (int n = 0; n < sorted.Count; n++)
                {
                    var i = sorted[n];
                    i.Bounds(out var a, out var b);
                    float want = c0 + (c1 - c0) * n / (sorted.Count - 1);
                    i.NewPos += Offset(axis, want - (Get(a, axis) + Get(b, axis)) * 0.5f);
                }
            }
            return true;
        }

        //Give every item the rotation of the anchor (or just its heading, keeping each item's own tilt)
        public void MatchRotation(bool yawOnly)
        {
            if (Anchor == null) return;
            float ayaw = YawOf(Anchor.NewRot);
            foreach (var i in Items)
            {
                if (i == Anchor) continue;
                if (yawOnly)
                {
                    i.NewRot = Quaternion.Normalize(YawQuat(ayaw - YawOf(i.NewRot)) * i.NewRot);
                }
                else
                {
                    i.NewRot = Anchor.NewRot;
                }
            }
        }

        //Turn every other item to face the anchor, e.g. chairs towards a table. flip turns them the other way
        //round for models that face -Y.
        public void FaceAnchor(bool flip)
        {
            if (Anchor == null) return;
            Anchor.Bounds(out var amin, out var amax);
            var target = (amin + amax) * 0.5f;
            foreach (var i in Items)
            {
                if (i == Anchor) continue;
                i.Bounds(out var bmin, out var bmax);
                var d = target - (bmin + bmax) * 0.5f;
                if ((d.X * d.X + d.Y * d.Y) < 1e-6f) continue;
                float yaw = (float)Math.Atan2(-d.X, d.Y) + (flip ? (float)Math.PI : 0);
                i.NewRot = Quaternion.Normalize(YawQuat(yaw - YawOf(i.NewRot)) * i.NewRot);
            }
        }

        //Drop each item straight down onto whatever collision or model is under it. With alignToSurface it
        //also tilts the item to follow the slope (up to maxTilt degrees).
        public int FitToSurface(Space space, bool alignToSurface, float maxTilt)
        {
            int fitted = 0;
            var moving = new HashSet<YmapEntityDef>(Items.Select(i => i.Entity));
            foreach (var i in Items)
            {
                i.Bounds(out var bmin, out var bmax);
                var centre = (bmin + bmax) * 0.5f;
                float height = bmax.Z - bmin.Z;
                var origin = new Vector3(centre.X, centre.Y, bmin.Z + Math.Max(height * 0.5f, 0.3f));
                if (!CastSkippingSelf(space, moving, origin, -Vector3.UnitZ, 60.0f, out var hit))
                {
                    origin.Z = bmax.Z + 0.5f;
                    if (!CastSkippingSelf(space, moving, origin, -Vector3.UnitZ, 60.0f, out hit)) continue;
                }
                if (alignToSurface)
                {
                    var n = Vector3.Normalize(hit.Normal);
                    float tilt = (float)(Math.Acos(Math.Max(-1f, Math.Min(1f, n.Z))) * 180.0 / Math.PI);
                    if ((tilt > 0.5f) && (tilt <= maxTilt))
                    {
                        float yaw = YawOf(i.NewRot);
                        var axis = Vector3.Cross(Vector3.UnitZ, n);
                        if (axis.LengthSquared() > 1e-8f)
                        {
                            axis.Normalize();
                            var tiltq = Quaternion.RotationAxis(axis, (float)(tilt * Math.PI / 180.0));
                            i.NewRot = Quaternion.Normalize(tiltq * YawQuat(yaw));
                        }
                    }
                    else
                    {
                        i.NewRot = YawQuat(YawOf(i.NewRot));
                    }
                    i.Bounds(out bmin, out bmax);
                }
                i.NewPos += new Vector3(0, 0, hit.Position.Z - bmin.Z);
                fitted++;
            }
            return fitted;
        }

        //Push each item sideways until it touches the nearest wall/surface (within maxDist), optionally turning it
        //to face away from that surface. Handy for doors, pictures, shelves, wall lights.
        public int SnapToWall(Space space, float maxDist, bool alignToWall, bool flip)
        {
            int snapped = 0;
            var moving = new HashSet<YmapEntityDef>(Items.Select(i => i.Entity));
            var dirs = new[] { Vector3.UnitX, -Vector3.UnitX, Vector3.UnitY, -Vector3.UnitY };
            foreach (var i in Items)
            {
                i.Bounds(out var bmin, out var bmax);
                var centre = (bmin + bmax) * 0.5f;
                var half = (bmax - bmin) * 0.5f;
                var origin = new Vector3(centre.X, centre.Y, bmin.Z + Math.Max((bmax.Z - bmin.Z) * 0.5f, 0.3f));
                bool found = false;
                float best = float.MaxValue;
                SpaceRayIntersectResult bestHit = new SpaceRayIntersectResult();
                Vector3 bestDir = Vector3.Zero;
                foreach (var dir in dirs)
                {
                    if (!CastSkippingSelf(space, moving, origin, dir, maxDist + Math.Max(half.X, half.Y), out var hit)) continue;
                    float ext = (Math.Abs(dir.X) > 0.5f) ? half.X : half.Y;
                    float gap = hit.HitDist - ext;
                    if ((gap < best) && (gap <= maxDist))
                    {
                        best = gap;
                        bestHit = hit;
                        bestDir = dir;
                        found = true;
                    }
                }
                if (!found) continue;
                i.NewPos += bestDir * best;
                if (alignToWall)
                {
                    var n = new Vector3(bestHit.Normal.X, bestHit.Normal.Y, 0);
                    if (n.LengthSquared() > 1e-6f)
                    {
                        n.Normalize();
                        float yaw = (float)Math.Atan2(-n.X, n.Y) + (flip ? (float)Math.PI : 0);
                        i.NewRot = Quaternion.Normalize(YawQuat(yaw - YawOf(i.NewRot)) * i.NewRot);
                    }
                }
                snapped++;
            }
            return snapped;
        }

        //Casts a ray, ignoring hits on the entities being moved (by restarting the ray just past them)
        private static bool CastSkippingSelf(Space space, HashSet<YmapEntityDef> self, Vector3 origin, Vector3 dir, float maxDist, out SpaceRayIntersectResult result)
        {
            result = new SpaceRayIntersectResult();
            float travelled = 0;
            var pos = origin;
            for (int attempt = 0; attempt < 8; attempt++)
            {
                var ray = new Ray(pos, dir);
                var hit = space.RayIntersect(ray, maxDist - travelled);
                if (!hit.Hit) return false;
                if ((hit.HitEntity != null) && self.Contains(hit.HitEntity))
                {
                    float step = hit.HitDist + 0.02f;
                    travelled += step;
                    pos += dir * step;
                    if (travelled >= maxDist) return false;
                    continue;
                }
                hit.HitDist += travelled;
                result = hit;
                return true;
            }
            return false;
        }
    }
}
