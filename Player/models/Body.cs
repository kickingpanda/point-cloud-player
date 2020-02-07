using System.Collections.Generic;

namespace Player.Models
{
    public struct Body
    {
        public bool bTracked;
        public List<Joint> lJoints;
        public List<Point2f> lJointsInColorSpace;
    }
}
