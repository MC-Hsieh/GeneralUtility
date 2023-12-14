using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

public partial class SevenSegmentArray : UserControl
{
    #region " Properties "

    private int _iElementWidth = 10;
    private float italicFactor = 0.0F;
    private bool _bShowDot = true;
    private string _strValue = null;
    private Color _tColorBackground = Color.DarkGray;
    private Color _tColorDark = Color.DimGray;
    private Color _tColorLight = Color.Red;
    private Padding _clsElementPadding;
    private SevenSegment[] _clsSegments = null;


    /// <summary>
    /// Background color of the LED array.
    /// </summary>
    [Category("自定"), Browsable(true), Description("自定義[背景]LED顏色")]
    public Color ColorBackground
    {
        get { return _tColorBackground; }
        set
        {
            _tColorBackground = value;
            UpdateSegments();
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
            UpdateSegments();
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
            UpdateSegments();
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
            UpdateSegments();
        }
    }


    /// <summary>
    /// Shear coefficient for italicizing the displays. Try a value like -0.1.
    /// </summary>
    public float ItalicFactor
    {
        get { return italicFactor; }
        set
        {
            italicFactor = value;
            UpdateSegments();
        }
    }

    /// <summary>
    /// Specifies if the decimal point LED is displayed.
    /// </summary>
    [Category("自定"), Browsable(true), Description("是否顯示小數點")]
    public bool DecimalShow
    {
        get { return _bShowDot; }
        set
        {
            _bShowDot = value;
            UpdateSegments();
        }
    }

    /// <summary>
    /// Number of seven-segment elements in this array.
    /// </summary>
    [Category("自定"), Browsable(true), Description("自定義LED長度")]
    public int ArrayCount
    {
        get { return _clsSegments.Length; }
        set
        {
            if ((value > 0) && (value <= 100))
                RecreateSegments(value);
        }
    }

    /// <summary>
    /// Padding that applies to each seven-segment element in the array.
    /// Tweak these numbers to get the perfect appearance for the array of your size.
    /// </summary>
    public Padding ElementPadding
    {
        get { return _clsElementPadding; }
        set
        {
            _clsElementPadding = value;
            UpdateSegments();
        }
    }

    /// <summary>
    /// The value to be displayed on the LED array. This can contain numbers,
    /// certain letters, and decimal points.
    /// </summary>
    [Category("自定"), Browsable(true), Description("自定義 7 段顯示數字(範圍: 0 ~ 9)")]
    public string Value
    {
        get { return _strValue; }
        set
        {
            _strValue = value;
            for (int i = 0; i < _clsSegments.Length; i++)
            {
                _clsSegments[i].CustomPattern = 0;
                _clsSegments[i].DecimalOn = false;
            }

            if (_strValue != null)
            {
                int segmentIndex = 0;
                for (int i = _strValue.Length - 1; i >= 0; i--)
                {
                    if (segmentIndex >= _clsSegments.Length) break;
                    if (_strValue[i] == '.')
                        _clsSegments[segmentIndex].DecimalOn = true;
                    else
                        _clsSegments[segmentIndex++].Value = _strValue[i].ToString();
                }
            }
        }
    }

    #endregion

    #region " Methods - New "

    public SevenSegmentArray()
    {
        SuspendLayout();
        Name = "SevenSegmentArray";
        Size = new Size(100, 25);
        Resize += new EventHandler(SevenSegmentArray_Resize);
        ResumeLayout(false);

        TabStop = false;
        _clsElementPadding = new Padding(4, 4, 4, 4);
        RecreateSegments(4);
    }

    #endregion

    #region " Methods - Create/Resize/Update/Paint "

    /// <summary>
    /// Change the number of elements in our LED array. This destroys
    /// the previous elements, and creates new ones in their place, applying
    /// all the current options to the new ones.
    /// </summary>
    /// <param name="count">Number of elements to create.</param>
    private void RecreateSegments(int count)
    {
        if (_clsSegments != null)
            for (int i = 0; i < _clsSegments.Length; i++) { _clsSegments[i].Parent = null; _clsSegments[i].Dispose(); }

        if (count <= 0) return;
        _clsSegments = new SevenSegment[count];

        for (int i = 0; i < count; i++)
        {
            _clsSegments[i] = new SevenSegment();
            _clsSegments[i].Parent = this;
            _clsSegments[i].Top = 0;
            _clsSegments[i].Height = Height;
            _clsSegments[i].Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            _clsSegments[i].Visible = true;
        }

        ResizeSegments();
        UpdateSegments();
        Value = _strValue;
    }

    /// <summary>
    /// Align the elements of the array to fit neatly within the
    /// width of the parent control.
    /// </summary>
    private void ResizeSegments()
    {
        int segWidth = Width / _clsSegments.Length;
        for (int i = 0; i < _clsSegments.Length; i++)
        {
            _clsSegments[i].Left = Width * (_clsSegments.Length - 1 - i) / _clsSegments.Length;
            _clsSegments[i].Width = segWidth;
        }
    }

    /// <summary>
    /// Update the properties of each element with the properties
    /// we have stored.
    /// </summary>
    private void UpdateSegments()
    {
        for (int i = 0; i < _clsSegments.Length; i++)
        {
            _clsSegments[i].ColorBackground = _tColorBackground;
            _clsSegments[i].ColorDark = _tColorDark;
            _clsSegments[i].ColorLight = _tColorLight;
            _clsSegments[i].ElementWidth = _iElementWidth;
            //_clsSegments[i].ItalicFactor = italicFactor;
            _clsSegments[i].DecimalShow = _bShowDot;
            _clsSegments[i].Padding = _clsElementPadding;
        }
    }

    private void SevenSegmentArray_Resize(object sender, EventArgs e)
    {
        ResizeSegments();
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        e.Graphics.Clear(_tColorBackground);
    }

    #endregion

}
