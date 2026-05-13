using dc.ui;
using ModCore.Utilities;

namespace DeadCellsArchipelago {
    public class Line
    {
        public UIBox bgBox;

        public Line(double width, double height, double x, double y, int color)
        {
            bgBox = new UIBox("boxMain".AsHaxeString(), width, height, 0, 0);
            bgBox.colorizeSG(color);
            bgBox.x = x;
            bgBox.y = y;
        }

        public virtual void AddParent(dc.h2d.Object parent)
        {
            parent.addChild(bgBox);
        }
    }
}