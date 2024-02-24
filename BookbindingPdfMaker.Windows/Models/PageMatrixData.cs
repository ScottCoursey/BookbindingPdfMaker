﻿namespace BookbindingPdfMaker.Models
{
    class PageMatrixData
    {
        public int SheetNum { get; set; }
        public PageFrontOrBack FrontOrBack { get; set; }
        public int PageNumTopLeft { get; set; }
        public int PageNumTopRight { get; set; }
        public int PageNumBottomLeft { get; set; }
        public int PageNumBottomRight { get; set; }

        public void ClearPageNumIfOver(int maxValue)
        {
            if (PageNumTopLeft > maxValue)
            {
                PageNumTopLeft = 0;
            }

            if (PageNumTopRight > maxValue)
            {
                PageNumTopRight = 0;
            }

            if (PageNumBottomLeft > maxValue)
            {
                PageNumBottomLeft = 0;
            }

            if (PageNumBottomRight > maxValue)
            {
                PageNumBottomRight = 0;
            }
        }
    }
}
