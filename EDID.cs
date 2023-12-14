using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeneralUtility.Net;

namespace GeneralUtility
{
    public class EDID
    {
        /// <summary> (UDP)取得EDID CODE </summary>
        /// <param name="strProductID">Product Name</param>
        /// <param name="strPanelID">Panel ID</param>
        /// <param name="strEDID_CTR_IP">EDID Server IP</param>
        public string GetEDIDCode(string strProductID, string strPanelID, string strEDID_CTR_IP)
        {
            //_clsLog.Info("GetEDIDCode");

            //_clsVariable.strChildCode = "";
            //txt_ChildCode.Text = "";
            //_clsVariable.strEDIDcode = "";
            //_IAUX_Tool.strEDIDcode = "";

            string strCommand = "^" + strProductID + "," + strPanelID + "$";
            string strEcho = UDPClient.UDPClientSendData(strEDID_CTR_IP, 1670, strCommand, Encoding.UTF8, 200);

            if (strEcho == "@%" || strEcho == null) return null;

            char[] cSplits = new char[] { '@', '%', '|' };
            string[] strSplits = strEcho.Split(cSplits, StringSplitOptions.RemoveEmptyEntries);

            if (strSplits.Length == 2) return strSplits[1];
            else return null;

            //{

            //    _clsVariable.strChildCode = strSplits[0];
            //    _clsVariable.strEDIDcode = strSplits[1];
            //    txt_ChildCode.Text = _clsVariable.strChildCode;
            //    xled_EDID.NowStatus = XStatusLED.Status.On;
            //    _clsLog.Info("GetEDIDCode Success : " + _clsVariable.strEDIDcode);
            //}
        }

        /// <summary> Mouse_right_click has 3 mode to change </summary>
        public enum JUDGE
        {
            Judge_All,
            Judge_FirstHalf,
            Judge_LastHalf,
        }

        public JUDGE eJUDGE
        {
            set { _eJUDGE = value; }
            get { return _eJUDGE; }
        }
        private JUDGE _eJUDGE = JUDGE.Judge_All;

        /// <summary> (UDP)傳送EDID CODE 至EDID CTR並取得比對結果 </summary>
        /// <param name="strProductID">Product Name</param>
        /// <param name="strPanelID">Panel ID</param>
        /// <param name="strEDID">讀出來的EDID CODE</param>
        /// <returns>比對結果</returns>
        private bool CheckEDID_inEDIDCTR(string strProductID, string strPanelID, string strEDID, string strEDID_CTR_IP, JUDGE _eJUDGE)
        {
            //_clsLog.Info("CheckEDID_inEDIDCTR");

            string strCommand = "^" + strProductID + "," + strPanelID + "," + strEDID + "$";
            string strEcho = UDPClient.UDPClientSendData(strEDID_CTR_IP, 1670, strCommand, Encoding.UTF8, 200);

            if (strEcho == "@%" || strEcho == null) return false;

            strEcho = strEcho.Substring(1, strEcho.Length - 2);
            string[] bEcho = strEcho.Split(',');

            if (bEcho.Length != 2) return false;

            bool bJudge = false;

            switch (_eJUDGE)
            {
                case JUDGE.Judge_All:
                    if (bEcho[0] == "OK" && bEcho[1] == "OK") bJudge = true;
                    else bJudge = false;
                    break;

                case JUDGE.Judge_FirstHalf:
                    if (bEcho[0] == "OK") bJudge = true;
                    else bJudge = false;
                    break;

                case JUDGE.Judge_LastHalf:
                    if (bEcho[1] == "OK") bJudge = true;
                    else bJudge = false;
                    break;
            }
            return bJudge;
        }

        //public static byte GetComplement(byte[] bBytes, int iComplementIndex,byte bTargetValue)
        //{
        //    if (bBytes == null) return bTargetValue;
        //    if (bBytes.Length > 0)
        //    {
        //        for (int i = 0; i < bBytes.Length; i++)
        //        {
        //            if (i != iComplementIndex)
        //            {
        //                bTargetValue = (byte)(bTargetValue ^ bBytes[i]);
        //            }
        //        }
        //    }
        //    return bTargetValue;
        //}

        //public static byte GetComplement(string strByteString, int iComplementIndex, string bTargetValue)
        //{
        //    return GetComplement(StringToByteArray(strByteString), iComplementIndex, StringToByte(bTargetValue));
        //}

        //public static byte[] StringToByteArray(string hex)
        //{
        //    return Enumerable.Range(0, hex.Length)
        //                     .Where(x => x % 2 == 0)
        //                     .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
        //                     .ToArray();
        //}

        //public static byte StringToByte(string strByte)
        //{
        //    if (strByte == null) return 0x00;
        //    if (strByte.Length < 2) return 0x00;
        //    return Convert.ToByte(strByte.Substring(0,2), 16);
        //}
    }
}
