using GeneralUtility.IO;
using GeneralUtility.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GeneralUtility.License
{
    public class DeviceInfoLock
    {
        public string strSmBIOSUUID;
        public string strCPUID;
        public string strLock;
        public string strCreatTime;
        public string strTimeLock;
    }
    public class DEKey
    {
        public string strSmBIOSUUID;
        public string strCPUID;
        public string strLock;
        public string strLicenseName;
        public string strKeyCreatTime;
        public string strLastTime;
        public string strKeyMode;
        public string strKeyModeLock;
        public string strTryDueDate;
        public string strTryLock;
        public string strPermissions;
    }

    public class License
    {
        private const string strKey = "-DETOP-";
        private const string strLockFileName = "Device.lock";

        public static int CheckLock(string strKey ,ref DeviceInfoLock clsDeviceLock, string strPath = strLockFileName)
        {
            try
            {
                string[] strDeviceString = GeneralUtility.IO.XFile.ReadFile(strPath);
                if (strDeviceString == null) return 2;
                if (strDeviceString.Length < 1) return 2;
                byte[] strReals = GeneralUtility.Security.RSA.DecryptData2String(strDeviceString[0], strKey, RSA.StringType.Base64);
                GeneralUtility.IO.XFile.ReadXmlSerialize(strReals, ref clsDeviceLock);
                return CheckDeviceLock(clsDeviceLock);
            }
            catch(Exception ex)
            {
                return 2;
            }
        }

        public static void CreatLock()
        {
            if(!XFile.IsExist_File("UKEY.DE"))
            {
                MessageBox.Show("No UKEY", "NO UKEY");
                Application.Exit();
                return;
            }
            string strKey = XFile.ReadFile("UKEY.DE")[0];
            DeviceInfoLock clsDeviceLock = new DeviceInfoLock();
            clsDeviceLock = GetDeviceInfo();
            byte[] clsLockDatas = GeneralUtility.IO.XFile.WriteXmlSerializeToArray(clsDeviceLock);
            string strSaveString = GeneralUtility.Security.RSA.EncryptData2String(clsLockDatas, strKey, RSA.StringType.Base64);
            GeneralUtility.IO.XFile.WriteFile(strLockFileName, strSaveString);
        }

        private static int CheckDeviceLock(DeviceInfoLock clsDeviceLock)
        {
            int iReturn = 2;
            string strLock = GeneralUtility.Security.Hash.EncryptToHexString(Hash.EncryptType.SHA256, clsDeviceLock.strSmBIOSUUID + strKey + clsDeviceLock.strCPUID);
            string strTimeLock = GeneralUtility.Security.Hash.EncryptToHexString(Hash.EncryptType.SHA256, clsDeviceLock.strCreatTime + strKey);

            if (strLock == clsDeviceLock.strLock) iReturn--;
            if (strTimeLock == clsDeviceLock.strTimeLock) iReturn--;

            return iReturn;
        }

        private static DeviceInfoLock GetDeviceInfo()
        {
            DeviceInfoLock clsDeviceInfo = new DeviceInfoLock();

            clsDeviceInfo.strSmBIOSUUID = GeneralUtility.XWinApi.GetSmBIOSUUID();
            clsDeviceInfo.strCPUID = GeneralUtility.XWinApi.GetCpuID();
            clsDeviceInfo.strLock = GeneralUtility.Security.Hash.EncryptToHexString(Hash.EncryptType.SHA256, clsDeviceInfo.strSmBIOSUUID + strKey + clsDeviceInfo.strCPUID);
            clsDeviceInfo.strCreatTime = DateTime.Now.ToString("yyyyMMddHHmmss");
            clsDeviceInfo.strTimeLock = GeneralUtility.Security.Hash.EncryptToHexString(Hash.EncryptType.SHA256, clsDeviceInfo.strCreatTime + strKey);
            return clsDeviceInfo;
        }

        public static DEKey CreatKey(DeviceInfoLock clsLock,string strLicenseName,string strKeyMode,int iKeyCount,string strPermissions)
        {
            clsLock = GetDeviceInfo();
            DEKey clsKey = new DEKey();
            int iCount = (iKeyCount > 30) ? 30 : iKeyCount;
            clsKey.strSmBIOSUUID = clsLock.strSmBIOSUUID;
            clsKey.strCPUID = clsLock.strCPUID;
            clsKey.strLock = GeneralUtility.Security.Hash.EncryptToHexString(Hash.EncryptType.SHA256, clsKey.strCPUID + strKey + clsKey.strSmBIOSUUID);
            clsKey.strLicenseName = strLicenseName;
            clsKey.strKeyCreatTime = DateTime.Now.ToString("yyyyMMddHHmmss");
            clsKey.strLastTime = DateTime.Now.ToString("yyyyMMddHHmmss");
            clsKey.strKeyMode = strKeyMode;
            clsKey.strKeyModeLock = GeneralUtility.Security.Hash.EncryptToHexString(Hash.EncryptType.SHA256, clsKey.strKeyMode + clsKey.strCPUID) ;
            clsKey.strTryDueDate = (DateTime.Now.AddDays(Convert.ToDouble(iCount))).ToString("yyyyMMddHHmmss");
            clsKey.strTryLock = GeneralUtility.Security.Hash.EncryptToHexString(Hash.EncryptType.SHA256, clsKey.strTryDueDate + clsKey.strSmBIOSUUID);
            clsKey.strPermissions = strPermissions;

            return clsKey;
    }


        /// <summary>
        /// 認證使用將電腦Key讀出後丟入
        /// </summary>
        /// <param name="clsKey"></param>
        public static void CheckKey()
        {
            DeviceInfoLock clsDeviceLock = GetDeviceInfo();

            if (!XFile.IsExist_File("UKEY.Dekey"))
            {
                MessageBox.Show("No DeKey", "NO KEY");
                Application.Exit();
                return;
            }
            DEKey clsKey = new DEKey();
            string[] strDeviceString = GeneralUtility.IO.XFile.ReadFile("UKEY.Dekey");
            if (strDeviceString == null) return;
            if (strDeviceString.Length < 1) return;
            byte[] strReals = GeneralUtility.Security.Symmetric.Decrypt2StringB(Symmetric.Type.AES, strDeviceString[0], clsDeviceLock.strSmBIOSUUID, clsDeviceLock.strCPUID, Symmetric.StringType.Base64);
            GeneralUtility.IO.XFile.ReadXmlSerialize(strReals, ref clsKey);

            bool bDeciveOK = (clsKey.strLock == GeneralUtility.Security.Hash.EncryptToHexString(Hash.EncryptType.SHA256, clsDeviceLock.strCPUID + strKey + clsDeviceLock.strSmBIOSUUID));
            bool bKeyModeOK = (clsKey.strKeyModeLock == GeneralUtility.Security.Hash.EncryptToHexString(Hash.EncryptType.SHA256, clsKey.strKeyMode + clsDeviceLock.strCPUID));
            bool bTryOK = (clsKey.strTryLock == GeneralUtility.Security.Hash.EncryptToHexString(Hash.EncryptType.SHA256, clsKey.strTryDueDate + clsDeviceLock.strSmBIOSUUID));

            if (!bDeciveOK)
            {
                MessageBox.Show("Device Compare Error 裝置比對錯誤 請確認!", "裝置錯誤");
                Application.Exit();
                return;
            }
            if (bKeyModeOK)
            {
                if(clsKey.strKeyMode == "Trial")
                {
                    DateTime sDueDate = DateTime.ParseExact(clsKey.strTryDueDate, "yyyyMMddHHmmss", null);
                    DateTime sLastDate = DateTime.ParseExact(clsKey.strLastTime, "yyyyMMddHHmmss", null);
                    DateTime sCreatDate = DateTime.ParseExact(clsKey.strKeyCreatTime, "yyyyMMddHHmmss", null);
                    TimeSpan sTime1 = DateTime.Now - sCreatDate;
                    TimeSpan sTime2 = DateTime.Now - sLastDate;        
                    TimeSpan sTime3 = DateTime.Now - sDueDate;
                    if(sTime1.Seconds > 0 && sTime2.Seconds > 0 && sTime3.Seconds < 0)
                    {
                        MessageBox.Show("試用期限剩餘 " + Convert.ToInt32(sTime3.TotalDays).ToString("D2") + " 天", "試用");
                        //認證成功
                    }
                    else
                    {
                        MessageBox.Show("試用期已過!", "試用");
                        Application.Exit(); 
                    }
                }
            }
        }

    }


}
