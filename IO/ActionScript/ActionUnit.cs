using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace GeneralUtility.IO.ActionScript
{
    [Serializable]
    public class ActionUnit
    {
        #region " Definition "

        public enum eActionType
        {
            Keyboard,
            Mouse,
            Other,
            None
        }

        public enum eKeyboardType
        {
            Down,
            Up,
            Click,
            String,
            None
        }

        public enum eMouseType
        {
            LeftDown,
            LeftUp,
            LeftClick,
            LeftDoubleClick,
            RightDown,
            RightUp,
            RightClick,
            RightDoubleClick,
            Move,
            None,
        }

        public enum eOtherType
        {
            Second,
            MilliSecond,
            String,
            None,
        }

        #endregion

        #region " Property "

        public eActionType eAction
        {
            set { _ActionType = value; }
            get { return _ActionType; }
        }
        private eActionType _ActionType = eActionType.None;

        public eKeyboardType eKeyboard
        {
            set { _eKeyboardType = value; }
            get { return _eKeyboardType; }
        }
        private eKeyboardType _eKeyboardType = eKeyboardType.None;

        public eMouseType eMouse
        {
            set { _eMouseType = value; }
            get { return _eMouseType; }
        }
        private eMouseType _eMouseType = eMouseType.None;

        public eOtherType eOther
        {
            set { _eOtherType = value; }
            get { return _eOtherType; }
        }
        private eOtherType _eOtherType = eOtherType.None;

        public object[] strPara
        {
            set { _strPara = value; }
            get { return _strPara; }
        }
        private object[] _strPara;

        public static Dictionary<string, string> VariableDic
        {
            set { _VariableDic = value; }
            get { return _VariableDic; }
        }
        private static Dictionary<string, string> _VariableDic;

        #endregion

        #region " Methods - New "

        public ActionUnit(eKeyboardType eKeyboard,params object[] strPara)
        {
            _ActionType = eActionType.Keyboard;
            _eKeyboardType = eKeyboard;
            _strPara = strPara;
        }

        public ActionUnit(eMouseType eMouse, params object[] strPara)
        {
            _ActionType = eActionType.Mouse;
            _eMouseType = eMouse;
            _strPara = strPara;
        }

        public ActionUnit(eOtherType eOther, params object[] strPara)
        {
            _ActionType = eActionType.Other;
            _eOtherType = eOther;
            _strPara = strPara;
        }

        public ActionUnit()
        {
        }

        #endregion

        #region " Methods - Action"

        public bool Action()
        {
            bool bReturn = false;

            switch (_ActionType)
            {
                case eActionType.Keyboard:
                    bReturn = ActionKeyboard();
                    break;
                case eActionType.Mouse:
                    bReturn = ActionMouse();
                    break;
                case eActionType.Other:
                    bReturn = ActionOther();
                    break;
            }
            return bReturn;

        }

        private bool ActionKeyboard()
        {
            bool bReturn = false;

            if (_strPara != null)
            {
                if (_strPara.Length > 0)
                {
                    switch (_eKeyboardType)
                    {
                        case eKeyboardType.Down:
                            if (TypeCheck(_strPara[0], typeof(Int32), typeof(Keys)))
                            {
                                Keyboard.KeyDown((byte)((int)_strPara[0]));
                                bReturn = true;
                            }
                            break;
                        case eKeyboardType.Up:
                            if (TypeCheck(_strPara[0], typeof(Int32), typeof(Keys)))
                            {
                                Keyboard.KeyUp((byte)((int)_strPara[0]));
                                bReturn = true;
                            }
                            break;
                        case eKeyboardType.Click:
                            if (TypeCheck(_strPara[0], typeof(Int32), typeof(Keys)))
                            {
                                Keyboard.KeyClick((byte)((int)_strPara[0]));
                                bReturn = true;
                            }
                            break;
                        case eKeyboardType.String:
                            if (TypeCheck(_strPara[0], typeof(string)))
                            {
                                string strShowData = (string)_strPara[0];
                                if (strShowData.Substring(0, 1) == "$")
                                {
                                    if (_VariableDic != null)
                                    {
                                        if (_VariableDic.ContainsKey(strShowData))
                                        {
                                            SendKeys.SendWait(_VariableDic[strShowData]);
                                        }
                                        else
                                        {
                                            SendKeys.SendWait(strShowData);
                                        }
                                    }
                                    else
                                    {
                                        SendKeys.SendWait(strShowData);
                                    }
                                }
                                else
                                {
                                    SendKeys.SendWait(strShowData);
                                }
                                bReturn = true;
                            }
                            break;
                    }
                }
            }

            return bReturn;
        }

        private bool ActionMouse()
        {
            bool bReturn = false;

            if (_strPara != null)
            {
                    switch (_eMouseType)
                    {
                        case eMouseType.LeftDown:
                            Mouse.LeftDown();
                            bReturn = true;
                            break;
                        case eMouseType.LeftUp:
                            Mouse.LeftUp();
                            bReturn = true;
                            break;
                        case eMouseType.LeftClick:
                            Mouse.LeftClick();
                            bReturn = true;
                            break;
                        case eMouseType.LeftDoubleClick:
                            Mouse.LeftDoubleClick();
                            bReturn = true;
                            break;
                        case eMouseType.RightDown:
                            Mouse.RightDown();
                            bReturn = true;
                            break;
                        case eMouseType.RightUp:
                            Mouse.RightUp();
                            bReturn = true;
                            break;
                        case eMouseType.RightClick:
                            Mouse.RightClick();
                            bReturn = true;
                            break;
                        case eMouseType.RightDoubleClick:
                            Mouse.RightDoubleClick();
                            bReturn = true;
                            break;
                        case eMouseType.Move:
                            if (_strPara.Length > 1)
                            {
                                if (TypeCheck(_strPara[0], typeof(Int32)) && TypeCheck(_strPara[1], typeof(Int32)))
                                {
                                    Mouse.SetMousePoint(new System.Drawing.Point((int)_strPara[0], (int)_strPara[0]));
                                    bReturn = true;
                                }
                            }
                            break;

                    }
            }

            return bReturn;
        }

        private bool ActionOther()
        {
            bool bReturn = false;

            if (_strPara != null)
            {
                if (_strPara.Length > 0)
                {
                    switch (_eOtherType)
                    {
                        case eOtherType.Second:
                            if (TypeCheck(_strPara[0], typeof(Int32)))
                            {
                                Thread.Sleep((int)_strPara[0] * 1000);
                                bReturn = true;
                            }
                            break;
                        case eOtherType.MilliSecond:
                            if (TypeCheck(_strPara[0], typeof(Int32)))
                            {
                                Thread.Sleep((int)_strPara[0]);
                                bReturn = true;
                            }
                            break;
                    }
                    
                }
            }

            return bReturn;
        }

        #endregion

        #region " Methods - Function "

        /// <summary>類型判段</summary>
        /// <param name="check">預判斷類型</param>
        /// <param name="clsTypes">認可類型</param>
        /// <returns>預判斷類型為認可類型 >> True</returns>
        private bool TypeCheck(object check, params Type[] clsTypes)
        {
            foreach (Type clsType in clsTypes)
            {
                if (check.GetType() == clsType)
                    return true;
            }
            return false;
        }

        #endregion
    }
}
