using dc.h2d;

namespace DeadCellsArchipelago {
    public class BiomeLine : Line
    {
        public BiomeCell cell1;
        public BiomeCell cell2;
        public BiomeCell cell3;
        public Flow flow;

        public BiomeLine(double x, double y, string level1, string level2, string level3)
            : base(0, 0, x, y, 0)
        {
            flow = new Flow(null)
            {
                x = x,
                y = y
            };
            flow.set_horizontalSpacing(50);

            cell1 = new BiomeCell(0, 0, flow, level1);
            cell2 = new BiomeCell(0, 0, flow, level2);
            cell3 = new BiomeCell(0, 0, flow, level3);
        }

        public override void AddParent(dc.h2d.Object parent)
        {
            parent.addChild(flow);
        }

        public void Highlight(int index)
        {
            switch (index)
            {
                case 0:
                    cell1.Highlight();
                    break;
                case 1:
                    cell2.Highlight();
                    break;
                case 2:
                    cell3.Highlight();
                    break;
            }
        }

        public void StopHighlight(int index)
        {
            switch (index)
            {
                case 0:
                    cell1.StopHighlight();
                    break;
                case 1:
                    cell2.StopHighlight();
                    break;
                case 2:
                    cell3.StopHighlight();
                    break;
            }
        }

        public void Locked(int index)
        {
            switch (index)
            {
                case 0:
                    cell1.Locked();
                    break;
                case 1:
                    cell2.Locked();
                    break;
                case 2:
                    cell3.Locked();
                    break;
            }
        }

        public void SetPopUpTracker(int index)
        {
            switch (index)
            {
                case 0:
                    cell1.SetPopUpTracker();
                    break;
                case 1:
                    cell2.SetPopUpTracker();
                    break;
                case 2:
                    cell3.SetPopUpTracker();
                    break;
            }
        }
    }
}