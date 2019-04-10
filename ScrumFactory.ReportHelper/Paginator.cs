using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows;
using System.Windows.Media;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ScrumFactory.ReportHelper {

    /// <summary>
    /// This paginator provides document headers, footers and repeating table headers 
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class Paginator : DocumentPaginator {


        public Paginator(FlowDocument document, Definition def) {


            this.paginator = ((IDocumentPaginatorSource)document).DocumentPaginator;
            this.definition = def;

            def.Margins = document.PagePadding;

            def.FooterHeight = 40;
            paginator.PageSize = def.ContentSize;

            // Change page size of the document to
            // the size of the content area
            document.ColumnWidth = definition.ContentSize.Width; // Prevent columns
            document.PageWidth = definition.ContentSize.Width;
            document.PageHeight = definition.ContentSize.Height;
            document.PagePadding = new Thickness(0);

        }        

        private DocumentPaginator paginator;
        private Definition definition;

        public override DocumentPage GetPage(int pageNumber) {



            // Use default paginator to handle pagination
            Visual originalPage = paginator.GetPage(pageNumber).Visual;

            ContainerVisual visual = new ContainerVisual();
            ContainerVisual pageVisual = new ContainerVisual() { Transform = new TranslateTransform(definition.ContentOrigin.X, definition.ContentOrigin.Y) };
            pageVisual.Children.Add(originalPage);
            visual.Children.Add(pageVisual);

            // Create headers and footers
            if (definition.Header != null) {
                visual.Children.Add(CreateHeaderFooterVisual(definition.Header, definition.HeaderRect, pageNumber));
            }
            if (definition.Footer != null) {
                visual.Children.Add(CreateHeaderFooterVisual(definition.Footer, definition.FooterRect, pageNumber));
            }

            return new DocumentPage(
                visual,
                definition.PageSize,
                new Rect(new Point(), definition.PageSize),
                new Rect(definition.ContentOrigin, definition.ContentSize)
            );
        }

        /// <summary>
        /// Creates a visual to draw the header/footer
        /// </summary>
        /// <param name="draw"></param>
        /// <param name="bounds"></param>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        private Visual CreateHeaderFooterVisual(DrawHeaderFooter draw, Rect bounds, int pageNumber) {
            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext context = visual.RenderOpen()) {                
                draw(context, bounds, pageNumber, PageCount, definition.Title, definition.Logo, definition.PageSize.Width, definition.HeaderBGBrush, definition.HeaderBrush);
            }
            return visual;
        }


        #region DocumentPaginator members

        public override bool IsPageCountValid {
            get { return paginator.IsPageCountValid; }
        }

        public override int PageCount {
            get { return paginator.PageCount; }
        }

        public override Size PageSize {
            get {
                return paginator.PageSize;
            }
            set {
                paginator.PageSize = value;
            }
        }

        public override IDocumentPaginatorSource Source {
            get { return paginator.Source; }
        }

        #endregion


        public class Definition {

            public Definition() {
                this.Title = "";
                this.Header = DefaultHeader;
                this.Footer = DefaultFooter;
                this.HeaderBGBrush = Brushes.Transparent;
                this.HeaderBrush = Brushes.Black;
            }

            #region Page sizes

            /// <summary>
            /// PageSize in DIUs
            /// </summary>
            public Size PageSize {
                get { return _PageSize; }
                set { _PageSize = value; }
            }
            private Size _PageSize = new Size(793.5987, 1122.3987); // Default: A4

            /// <summary>
            /// Margins
            /// </summary>
            public Thickness Margins {
                get { return _Margins; }
                set { _Margins = value; }
            }
            private Thickness _Margins = new Thickness(60, 40, 40, 40); // Default: 1" margins

            /// <summary>
            /// Space reserved for the header in DIUs
            /// </summary>
            public double HeaderHeight {
                get { return _HeaderHeight; }
                set { _HeaderHeight = value; }
            }
            private double _HeaderHeight;

            /// <summary>
            /// Space reserved for the footer in DIUs
            /// </summary>
            public double FooterHeight {
                get { return _FooterHeight; }
                set { _FooterHeight = value; }
            }
            private double _FooterHeight;

            #endregion

            public BitmapImage Logo;
            public string Title;
            public DrawHeaderFooter Header, Footer;
            public Brush HeaderBGBrush;
            public Brush HeaderBrush;

            internal static void DrawDefaultHeader(DrawingContext context, Rect bounds, int pageNr, int pageCount, string title, BitmapImage logo, double pageWidth, Brush bg, Brush color) {                
                context.DrawRectangle(bg, null, new Rect(0,0, pageWidth, 24));
                FormattedText text = new FormattedText(title, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Helvetica"), 10, color);
                context.DrawText(text, new Point(bounds.Right - text.Width, bounds.Top + 6));                                
            }

            internal static void DrawDefaultFooter(DrawingContext context, Rect bounds, int pageNr, int pageCount, string title, BitmapImage logo, double pageWidth, Brush bg, Brush color) {
                context.DrawRectangle(bg, null, new Rect(0, bounds.Top, pageWidth, bounds.Height));                
                double logoSpace = 0;
                if (logo != null) {
                    double height = 32;
                    double width = logo.Width * (height / logo.Height);
                    context.DrawImage(logo, new Rect(bounds.Right - width, bounds.Top + 6, width, height));
                    logoSpace = width + 10;
                }
                FormattedText text = new FormattedText((pageNr + 1) + "/" + pageCount, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Helvetica"), 12, color);
                context.DrawText(text, new Point(bounds.Right - text.Width - logoSpace, bounds.Top + 12));
            }

            public static DrawHeaderFooter DefaultHeader = new DrawHeaderFooter(DrawDefaultHeader);
            public static DrawHeaderFooter DefaultFooter = new DrawHeaderFooter(DrawDefaultFooter);


            #region Some convenient helper properties

            internal Size ContentSize {
                get {
                    return new Size(PageSize.Width - (Margins.Left + Margins.Right), PageSize.Height - (Margins.Top + Margins.Bottom + HeaderHeight + FooterHeight));                    
                }
            }

            internal Point ContentOrigin {
                get {
                    return new Point(
                        Margins.Left,
                        Margins.Top + HeaderRect.Height
                    );
                }
            }

            internal Rect HeaderRect {
                get {
                    return new Rect(
                        Margins.Left, 0,
                        ContentSize.Width, HeaderHeight
                    );
                }
            }

            internal Rect FooterRect {
                get {
                    return new Rect(Margins.Left, ContentOrigin.Y + ContentSize.Height, ContentSize.Width, PageSize.Height);
                }
            }

            

            #endregion

        }

        /// <summary>
        /// Allows drawing headers and footers
        /// </summary>
        /// <param name="context">This is the drawing context that should be used</param>
        /// <param name="bounds">The bounds of the header. You can ignore these at your own peril</param>
        /// <param name="pageNr">The page nr (0-based)</param>
        public delegate void DrawHeaderFooter(DrawingContext context, Rect bounds, int pageNr, int pageCount, string title, BitmapImage logo, double pageWidth, Brush bg, Brush color);

    }

}
