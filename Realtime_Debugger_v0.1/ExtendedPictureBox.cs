using System;
using System.Windows.Forms;
using System.Drawing;

namespace Symplus.Controls
{
    /// <summary>
    /// PictureBox that treats vast size of virtual screen.
    /// You can use virtual geometry over 32767 that is maximum width/height of PictureBox control.
    /// Paint event and Mouse event are overridden.
    /// 
    /// (jp)仮想空間の座標系をもつPictureBox
    /// 
    /// ! Strings in comments after '(jp)' using Japanese characters.
    /// 
    /// Copyright 2005-, Ikuo Obataya, Symplus corp. Japan
    /// </summary>
    public class ExtendedPictureBox:PictureBox
    {
        #region Fields and Properties

        /// <summary>
        /// Custom paint event handler that contains "ClippingInVirtual".
        /// 
        /// (jp)ClippingInVirtualプロパティを含むイベントハンドラー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void ExpandedPaintEventHandler(object sender, ExtendedPaintEventArgs e);

        /// <summary>
        /// Custom event on paint.
        /// 
        /// (jp)仮想空間を考慮した描画イベント
        /// </summary>
        public event ExpandedPaintEventHandler ExpandedOnPaint;
        /// <summary>
        /// Custom event on mouse-move.
        /// e.X and e.Y are point in virtual screen.
        /// 
        /// (jp)仮想空間の座標を返すマウス移動イベント
        /// </summary>
        public event MouseEventHandler MouseMoveInVirtual;


        private Point virtualPoint = new Point(0, 0);
        /// <summary>
        /// Location of PictureBox in virtual screen.
        /// 
        /// (jp)仮想空間におけるPictureBoxの位置
        /// </summary>
        public Point VirtualPoint 
        {
            get { return virtualPoint; }
            set { virtualPoint = value; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Override
        /// </summary>
        /// <param name="pe"></param>
        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);

            if (ExpandedOnPaint != null)
                OnExpandedPaint(pe);
        }

        /// <summary>
        /// PaintEvent considering virtual screen geometry.
        /// 
        /// (jp)仮想空間を考慮したペイントイベント
        /// </summary>
        /// <param name="e"></param>
        protected void OnExpandedPaint(PaintEventArgs e)
        {
            /// Set offset.
            e.Graphics.TranslateTransform(-(float)virtualPoint.X, -(float)virtualPoint.Y);
            ExpandedOnPaint(this, new ExtendedPaintEventArgs(e.Graphics, e.ClipRectangle, new Rectangle(virtualPoint.X, virtualPoint.Y, this.ClientRectangle.Width, this.ClientRectangle.Height)));
        }

        /// <summary>
        /// Override
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (MouseMoveInVirtual != null)
                OnMouseMoveInVirtual(e);
        }

        /// <summary>
        /// MouseMoveEvent considering virtual screen geometry.
        /// The values of e.X and e.Y are that in virtual screen.
        /// 
        /// (jp)e.X, e.Yは仮想空間上の点を示す。
        /// </summary>
        /// <param name="e"></param>
        protected void OnMouseMoveInVirtual(MouseEventArgs e)
        {
            MouseMoveInVirtual(this, new MouseEventArgs(e.Button, e.Clicks, e.X + virtualPoint.X, e.Y + virtualPoint.Y, e.Delta));
        }

        /// <summary>
        /// Return point in virtual screen from cursor position
        /// 
        /// (jp)カーソル位置から仮想空間内の座標を返す。
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public Point PointToVirtual(Point pt) 
        {
            Point clientPt = this.PointToClient(pt);
            return new Point(clientPt.X + virtualPoint.X, clientPt.Y + virtualPoint.Y);
        }
        #endregion
    }

    #region EventArgs
    /// <summary>
    /// Paint on virtual screen
    /// </summary>
    public class ExtendedPaintEventArgs : PaintEventArgs
    {
        private Rectangle clippingInVirtual;
        /// <summary>
        /// Clipping rectangle in virtual screen
        /// 
        /// (jp)仮想空間上のクリップ矩形
        /// </summary>
        public Rectangle ClippingInVirtual
        {
            get { return clippingInVirtual; }
        }

        /// <summary>
        /// Clipping block in virtual screen
        /// (BlockSize: width = 0xFFF, height = 0xFFF)
        /// 
        /// (jp)仮想空間上のクリップブロック矩形
        /// </summary>
        public Rectangle ClippingBlock 
        {
            get
            {
                return Rectangle.FromLTRB(
                    clippingInVirtual.Left & 0x7FFFF000,
                    clippingInVirtual.Top & 0x7FFFF000,
                    clippingInVirtual.Right & 0x7FFFF000 + 0xFFF,
                    clippingInVirtual.Bottom & 0x7FFFF000 + 0xFFF);
            }
        }

        public ExtendedPaintEventArgs(Graphics g, Rectangle clipReal, Rectangle clipVirtual)
            : base(g, clipReal)
        {
            clippingInVirtual = clipVirtual;
        }
    }
    #endregion
}
