using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

public partial class SevenSegment : UserControl
{
    public enum ValuePattern
    {
        None = 0x0,
        Zero = 0x77,
        One = 0x24,
        Two = 0x5D,
        Three = 0x6D,
        Four = 0x2E,
        Five = 0x6B,
        Six = 0x7B,
        Seven = 0x25,
        Eight = 0x7F,
        Nine = 0x6F,
        A = 0x3F,
        B = 0x7A,
        C = 0x53,
        c = 0x58,
        D = 0x7C,
        E = 0x5B,
        F = 0x1B,
        G = 0x73,
        H = 0x3E,
        h = 0x3A,
        i = 0x20,
        J = 0x74,
        L = 0x52,
        N = 0x38,
        o = 0x78,
        P = 0x1F,
        Q = 0x2F,
        R = 0x18,
        T = 0x5A,
        U = 0x76,
        u = 0x70,
        Y = 0x6E,
        Dash = 0x8,
        Equals = 0x48,
        Degrees = 0xF,
        Apostrophe = 0x2,
        Quote = 0x6,
        RBracket = 0x65,
        Underscore = 0x40,
        Identical = 0x49,
        Not = 0x28
    }

    #region " Properties "

    private int _iGridHeight = 80;
    private int _iGridWidth = 48;
    private int _iElementWidth = 10;
    private float _fItalicFactor = 0.0F;
    private Color _tColorBackground = Color.DarkGray;
    private Color _tColorDark = Color.DimGray;
    private Color _tColorLight = Color.Red;

    private string _strValue = null;
    private bool showDot = true, dotOn = false;
    private bool _bShowColon = false, _bColonOn = false;
    private int _iCustomPattern = 0;
    private Point[][] _clsSegPoints;


    /// <summary>
    /// Background color of the 7-segment display.
    /// </summary>
    [Category("自定"), Browsable(true), Description("自定義[背景]LED顏色")]
    public Color ColorBackground
    {
        get { return _tColorBackground; }
        set
        {
            _tColorBackground = value;
            Invalidate();
        }
    }

    /// <summary>
    /// Color of inactive LED segments.
    /// </summary>
    [Category("自定"), Browsable(true), Description("自定義[預設]LED顏色")]
    public Color ColorDark
    {
        get { return _tColorDark; }
        set
        {
            _tColorDark = value;
            Invalidate();
        }
    }

    /// <summary>
    /// Color of active LED segments.
    /// </summary>
    [Category("自定"), Browsable(true), Description("自定義[顯示]LED顏色")]
    public Color ColorLight
    {
        get { return _tColorLight; }
        set
        {
            _tColorLight = value;
            Invalidate();
        }
    }


    /// <summary>
    /// Width of LED segments.
    /// </summary>
    [Category("自定"), Browsable(true), Description("自定義[調整]LED寬度")]
    public int ElementWidth
    {
        get { return _iElementWidth; }
        set
        {
            _iElementWidth = value;
            RecalculatePoints();
            Invalidate();
        }
    }

    /// <summary>
    /// Shear coefficient for italicizing the displays. Try a value like -0.1.
    /// </summary>
    //public float ItalicFactor 
    //{ 
    //    get { return _fItalicFactor; } 
    //    set
    //    { 
    //        _fItalicFactor = value; 
    //        Invalidate(); 
    //    } 
    //}


    /// <summary>
    /// Character to be displayed on the seven segments. Supported characters
    /// are digits and most letters.
    /// </summary>
    [Category("自定"), Browsable(true), Description("自定義 7 段顯示數字(範圍: 0 ~ 9)")]
    public string Value
    {
        get { return _strValue; }
        set
        {
            _iCustomPattern = 0;
            _strValue = value;
            Invalidate();
            if (value == null || value.Length == 0)
            {
                return;
            }
            //is it an integer?
            int tempValue;
            if (int.TryParse(value, out tempValue))
            {
                if (tempValue > 9) tempValue = 9; if (tempValue < 0) tempValue = 0;
                switch (tempValue)
                {
                    case 0: _iCustomPattern = (int)ValuePattern.Zero; break;
                    case 1: _iCustomPattern = (int)ValuePattern.One; break;
                    case 2: _iCustomPattern = (int)ValuePattern.Two; break;
                    case 3: _iCustomPattern = (int)ValuePattern.Three; break;
                    case 4: _iCustomPattern = (int)ValuePattern.Four; break;
                    case 5: _iCustomPattern = (int)ValuePattern.Five; break;
                    case 6: _iCustomPattern = (int)ValuePattern.Six; break;
                    case 7: _iCustomPattern = (int)ValuePattern.Seven; break;
                    case 8: _iCustomPattern = (int)ValuePattern.Eight; break;
                    case 9: _iCustomPattern = (int)ValuePattern.Nine; break;
                }
            }
            else
            {
                //is it a letter?
                switch (value[0])
                {
                    case 'A':
                    case 'a': _iCustomPattern = (int)ValuePattern.A; break;
                    case 'B':
                    case 'b': _iCustomPattern = (int)ValuePattern.B; break;
                    case 'C': _iCustomPattern = (int)ValuePattern.C; break;
                    case 'c': _iCustomPattern = (int)ValuePattern.c; break;
                    case 'D':
                    case 'd': _iCustomPattern = (int)ValuePattern.D; break;
                    case 'E':
                    case 'e': _iCustomPattern = (int)ValuePattern.E; break;
                    case 'F':
                    case 'f': _iCustomPattern = (int)ValuePattern.F; break;
                    case 'G':
                    case 'g': _iCustomPattern = (int)ValuePattern.G; break;
                    case 'H': _iCustomPattern = (int)ValuePattern.H; break;
                    case 'h': _iCustomPattern = (int)ValuePattern.h; break;
                    case 'I': _iCustomPattern = (int)ValuePattern.One; break;
                    case 'i': _iCustomPattern = (int)ValuePattern.i; break;
                    case 'J':
                    case 'j': _iCustomPattern = (int)ValuePattern.J; break;
                    case 'L':
                    case 'l': _iCustomPattern = (int)ValuePattern.L; break;
                    case 'N':
                    case 'n': _iCustomPattern = (int)ValuePattern.N; break;
                    case 'O': _iCustomPattern = (int)ValuePattern.Zero; break;
                    case 'o': _iCustomPattern = (int)ValuePattern.o; break;
                    case 'P':
                    case 'p': _iCustomPattern = (int)ValuePattern.P; break;
                    case 'Q':
                    case 'q': _iCustomPattern = (int)ValuePattern.Q; break;
                    case 'R':
                    case 'r': _iCustomPattern = (int)ValuePattern.R; break;
                    case 'S':
                    case 's': _iCustomPattern = (int)ValuePattern.Five; break;
                    case 'T':
                    case 't': _iCustomPattern = (int)ValuePattern.T; break;
                    case 'U': _iCustomPattern = (int)ValuePattern.U; break;
                    case 'u':
                    case 'µ':
                    case 'μ': _iCustomPattern = (int)ValuePattern.u; break;
                    case 'Y':
                    case 'y': _iCustomPattern = (int)ValuePattern.Y; break;
                    case '-': _iCustomPattern = (int)ValuePattern.Dash; break;
                    case '=': _iCustomPattern = (int)ValuePattern.Equals; break;
                    case '°': _iCustomPattern = (int)ValuePattern.Degrees; break;
                    case '\'': _iCustomPattern = (int)ValuePattern.Apostrophe; break;
                    case '"': _iCustomPattern = (int)ValuePattern.Quote; break;
                    case '[':
                    case '{': _iCustomPattern = (int)ValuePattern.C; break;
                    case ']':
                    case '}': _iCustomPattern = (int)ValuePattern.RBracket; break;
                    case '_': _iCustomPattern = (int)ValuePattern.Underscore; break;
                    case '≡': _iCustomPattern = (int)ValuePattern.Identical; break;
                    case '¬': _iCustomPattern = (int)ValuePattern.Not; break;
                }
            }
        }
    }

    /// <summary>
    /// Set a custom bit pattern to be displayed on the seven segments. This is an
    /// integer value where bits 0 through 6 correspond to each respective LED
    /// segment.
    /// </summary>
    [Category("自定"), Browsable(true), Description("自定義 7 段顯示區域位置(範圍: 0 ~ 6)")]
    public int CustomPattern
    {
        get { return _iCustomPattern; }
        set { _iCustomPattern = value; Invalidate(); }
    }

    /// <summary>
    /// Specifies if the decimal point LED is displayed.
    /// </summary>
    [Category("自定"), Browsable(true), Description("是否顯示小數點")]
    public bool DecimalShow
    {
        get { return showDot; }
        set { showDot = value; Invalidate(); }
    }
    /// <summary>
    /// Specifies if the decimal point LED is active.
    /// </summary>
    [Category("自定"), Browsable(true), Description("是否顯示小數點的顏色")]
    public bool DecimalOn
    {
        get { return dotOn; }
        set { dotOn = value; Invalidate(); }
    }

    /// <summary>
    /// Specifies if the colon LEDs are displayed.
    /// </summary>
    [Category("自定"), Browsable(true), Description("是否顯示冒號")]
    public bool ColonShow
    {
        get { return _bShowColon; }
        set { _bShowColon = value; Invalidate(); }
    }
    /// <summary>
    /// Specifies if the colon LEDs are active.
    /// </summary>
    [Category("自定"), Browsable(true), Description("是否顯示冒號的顏色")]
    public bool ColonOn
    {
        get { return _bColonOn; }
        set { _bColonOn = value; Invalidate(); }
    }

    #endregion

    #region " Methods - New "

    public SevenSegment()
    {
        SuspendLayout();
        Name = "SevenSegment";
        Size = new Size(32, 64);
        Paint += new PaintEventHandler(SevenSegment_Paint);
        Resize += new EventHandler(SevenSegment_Resize);
        ResumeLayout(false);

        TabStop = false;
        Padding = new Padding(4, 4, 4, 4);
        DoubleBuffered = true;

        _clsSegPoints = new Point[7][];
        for (int i = 0; i < 7; i++)
        {
            _clsSegPoints[i] = new Point[6];
        }

        RecalculatePoints();
    }

    #endregion

    #region " Methods - Calculat/Resize/Paddin/Paint "

    private void RecalculatePoints()
    {
        int halfHeight = _iGridHeight / 2, halfWidth = _iElementWidth / 2;

        int p = 0;
        _clsSegPoints[p][0].X = _iElementWidth + 1; _clsSegPoints[p][0].Y = 0;
        _clsSegPoints[p][1].X = _iGridWidth - _iElementWidth - 1; _clsSegPoints[p][1].Y = 0;
        _clsSegPoints[p][2].X = _iGridWidth - halfWidth - 1; _clsSegPoints[p][2].Y = halfWidth;
        _clsSegPoints[p][3].X = _iGridWidth - _iElementWidth - 1; _clsSegPoints[p][3].Y = _iElementWidth;
        _clsSegPoints[p][4].X = _iElementWidth + 1; _clsSegPoints[p][4].Y = _iElementWidth;
        _clsSegPoints[p][5].X = halfWidth + 1; _clsSegPoints[p][5].Y = halfWidth;

        p++;
        _clsSegPoints[p][0].X = 0; _clsSegPoints[p][0].Y = _iElementWidth + 1;
        _clsSegPoints[p][1].X = halfWidth; _clsSegPoints[p][1].Y = halfWidth + 1;
        _clsSegPoints[p][2].X = _iElementWidth; _clsSegPoints[p][2].Y = _iElementWidth + 1;
        _clsSegPoints[p][3].X = _iElementWidth; _clsSegPoints[p][3].Y = halfHeight - halfWidth - 1;
        _clsSegPoints[p][4].X = 4; _clsSegPoints[p][4].Y = halfHeight - 1;
        _clsSegPoints[p][5].X = 0; _clsSegPoints[p][5].Y = halfHeight - 1;

        p++;
        _clsSegPoints[p][0].X = _iGridWidth - _iElementWidth; _clsSegPoints[p][0].Y = _iElementWidth + 1;
        _clsSegPoints[p][1].X = _iGridWidth - halfWidth; _clsSegPoints[p][1].Y = halfWidth + 1;
        _clsSegPoints[p][2].X = _iGridWidth; _clsSegPoints[p][2].Y = _iElementWidth + 1;
        _clsSegPoints[p][3].X = _iGridWidth; _clsSegPoints[p][3].Y = halfHeight - 1;
        _clsSegPoints[p][4].X = _iGridWidth - 4; _clsSegPoints[p][4].Y = halfHeight - 1;
        _clsSegPoints[p][5].X = _iGridWidth - _iElementWidth; _clsSegPoints[p][5].Y = halfHeight - halfWidth - 1;

        p++;
        _clsSegPoints[p][0].X = _iElementWidth + 1; _clsSegPoints[p][0].Y = halfHeight - halfWidth;
        _clsSegPoints[p][1].X = _iGridWidth - _iElementWidth - 1; _clsSegPoints[p][1].Y = halfHeight - halfWidth;
        _clsSegPoints[p][2].X = _iGridWidth - 5; _clsSegPoints[p][2].Y = halfHeight;
        _clsSegPoints[p][3].X = _iGridWidth - _iElementWidth - 1; _clsSegPoints[p][3].Y = halfHeight + halfWidth;
        _clsSegPoints[p][4].X = _iElementWidth + 1; _clsSegPoints[p][4].Y = halfHeight + halfWidth;
        _clsSegPoints[p][5].X = 5; _clsSegPoints[p][5].Y = halfHeight;

        p++;
        _clsSegPoints[p][0].X = 0; _clsSegPoints[p][0].Y = halfHeight + 1;
        _clsSegPoints[p][1].X = 4; _clsSegPoints[p][1].Y = halfHeight + 1;
        _clsSegPoints[p][2].X = _iElementWidth; _clsSegPoints[p][2].Y = halfHeight + halfWidth + 1;
        _clsSegPoints[p][3].X = _iElementWidth; _clsSegPoints[p][3].Y = _iGridHeight - _iElementWidth - 1;
        _clsSegPoints[p][4].X = halfWidth; _clsSegPoints[p][4].Y = _iGridHeight - halfWidth - 1;
        _clsSegPoints[p][5].X = 0; _clsSegPoints[p][5].Y = _iGridHeight - _iElementWidth - 1;

        p++;
        _clsSegPoints[p][0].X = _iGridWidth - _iElementWidth; _clsSegPoints[p][0].Y = halfHeight + halfWidth + 1;
        _clsSegPoints[p][1].X = _iGridWidth - 4; _clsSegPoints[p][1].Y = halfHeight + 1;
        _clsSegPoints[p][2].X = _iGridWidth; _clsSegPoints[p][2].Y = halfHeight + 1;
        _clsSegPoints[p][3].X = _iGridWidth; _clsSegPoints[p][3].Y = _iGridHeight - _iElementWidth - 1;
        _clsSegPoints[p][4].X = _iGridWidth - halfWidth; _clsSegPoints[p][4].Y = _iGridHeight - halfWidth - 1;
        _clsSegPoints[p][5].X = _iGridWidth - _iElementWidth; _clsSegPoints[p][5].Y = _iGridHeight - _iElementWidth - 1;

        p++;
        _clsSegPoints[p][0].X = _iElementWidth + 1; _clsSegPoints[p][0].Y = _iGridHeight - _iElementWidth;
        _clsSegPoints[p][1].X = _iGridWidth - _iElementWidth - 1; _clsSegPoints[p][1].Y = _iGridHeight - _iElementWidth;
        _clsSegPoints[p][2].X = _iGridWidth - halfWidth - 1; _clsSegPoints[p][2].Y = _iGridHeight - halfWidth;
        _clsSegPoints[p][3].X = _iGridWidth - _iElementWidth - 1; _clsSegPoints[p][3].Y = _iGridHeight;
        _clsSegPoints[p][4].X = _iElementWidth + 1; _clsSegPoints[p][4].Y = _iGridHeight;
        _clsSegPoints[p][5].X = halfWidth + 1; _clsSegPoints[p][5].Y = _iGridHeight - halfWidth;
    }

    private void SevenSegment_Resize(object sender, EventArgs e)
    {
        Invalidate();
    }

    protected override void OnPaddingChanged(EventArgs e)
    {
        base.OnPaddingChanged(e);
        Invalidate();
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        e.Graphics.Clear(_tColorBackground);
    }

    private void SevenSegment_Paint(object sender, PaintEventArgs e)
    {
        int useValue = _iCustomPattern;

        Brush brushLight = new SolidBrush(_tColorLight);
        Brush brushDark = new SolidBrush(_tColorDark);

        // Define transformation for our container...
        RectangleF srcRect;

        int colonWidth = _iGridWidth / 4;

        if (_bShowColon)
        {
            srcRect = new RectangleF(0.0F, 0.0F, _iGridWidth + colonWidth, _iGridHeight);
        }
        else
        {
            srcRect = new RectangleF(0.0F, 0.0F, _iGridWidth, _iGridHeight);
        }
        RectangleF destRect = new RectangleF(Padding.Left, Padding.Top, Width - Padding.Left - Padding.Right, Height - Padding.Top - Padding.Bottom);

        // Begin graphics container that remaps coordinates for our convenience
        GraphicsContainer containerState = e.Graphics.BeginContainer(destRect, srcRect, GraphicsUnit.Pixel);

        Matrix trans = new Matrix();
        trans.Shear(_fItalicFactor, 0.0F);
        e.Graphics.Transform = trans;

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.PixelOffsetMode = PixelOffsetMode.Default;

        // Draw elements based on whether the corresponding bit is high
        e.Graphics.FillPolygon((useValue & 0x1) == 0x1 ? brushLight : brushDark, _clsSegPoints[0]);
        e.Graphics.FillPolygon((useValue & 0x2) == 0x2 ? brushLight : brushDark, _clsSegPoints[1]);
        e.Graphics.FillPolygon((useValue & 0x4) == 0x4 ? brushLight : brushDark, _clsSegPoints[2]);
        e.Graphics.FillPolygon((useValue & 0x8) == 0x8 ? brushLight : brushDark, _clsSegPoints[3]);
        e.Graphics.FillPolygon((useValue & 0x10) == 0x10 ? brushLight : brushDark, _clsSegPoints[4]);
        e.Graphics.FillPolygon((useValue & 0x20) == 0x20 ? brushLight : brushDark, _clsSegPoints[5]);
        e.Graphics.FillPolygon((useValue & 0x40) == 0x40 ? brushLight : brushDark, _clsSegPoints[6]);

        if (showDot)
            e.Graphics.FillEllipse(dotOn ? brushLight : brushDark, _iGridWidth - 1, _iGridHeight - _iElementWidth + 1, _iElementWidth, _iElementWidth);

        if (_bShowColon)
        {
            e.Graphics.FillEllipse(_bColonOn ? brushLight : brushDark, _iGridWidth + colonWidth - 4, _iGridHeight / 4 - _iElementWidth + 8, _iElementWidth, _iElementWidth);
            e.Graphics.FillEllipse(_bColonOn ? brushLight : brushDark, _iGridWidth + colonWidth - 4, _iGridHeight * 3 / 4 - _iElementWidth + 4, _iElementWidth, _iElementWidth);
        }

        e.Graphics.EndContainer(containerState);
    }

    #endregion
}

