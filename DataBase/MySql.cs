using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Reflection;
using System.Drawing;
using GeneralUtility.IO;

namespace GeneralUtility.DataBase
{
    public class MySql
    {
        /// <summary> 將資料庫的資料下載成Class
        /// Class 參數與資料庫欄位名稱相同
        /// 參數必須為public (不能為屬性)</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="strCommand"></param>
        /// <returns></returns>
        public static List<T> GetClassList<T>(MySqlConnection conn, string strCommand)
        {
            List<T> clsListT = new List<T>();
            T Tmp;
            Type clsType = typeof(T);

            if (!conn.Ping()) conn.Open();
            if (conn.Ping())
            {
                MySqlCommand cmd = new MySqlCommand(strCommand, conn);
                MySqlDataReader reader = cmd.ExecuteReader(); //execure the reader
                while (reader.Read())
                {
                    Tmp = (T)Activator.CreateInstance(typeof(T));
                    for (int i = 0; i < reader.VisibleFieldCount; i++)
                    {
                        if (reader.IsDBNull(i)) continue;
                        FieldInfo clsFieldInfo = Tmp.GetType().GetField(reader.GetName(i));
                        if (clsFieldInfo != null)
                        {
                            switch (clsFieldInfo.FieldType.Name)
                            {
                                case "String":
                                    Tmp.GetType().GetField(clsFieldInfo.Name).SetValue(Tmp, reader.GetString(i));
                                    break;
                                case "Int32":
                                    Tmp.GetType().GetField(clsFieldInfo.Name).SetValue(Tmp, reader.GetInt32(i));
                                    break;
                                case "Icon":
                                    Icon clsIcon = BaseTool.BytesToIcon(System.Convert.FromBase64String(reader.GetString(i)));
                                    Tmp.GetType().GetField(clsFieldInfo.Name).SetValue(Tmp, clsIcon);
                                    break;
                                case "Boolean":
                                    Tmp.GetType().GetField(clsFieldInfo.Name).SetValue(Tmp, reader.GetBoolean(i));
                                    break;
                                case "Byte[]":
                                    long iLength = reader.GetBytes(i, 0, null, 0, int.MaxValue);
                                    byte[] bData = new byte[iLength];
                                    reader.GetBytes(i, 0, bData, 0, bData.Length);
                                    Tmp.GetType().GetField(clsFieldInfo.Name).SetValue(Tmp, bData);
                                    break;

                                case "DateTime":
                                    Tmp.GetType().GetField(clsFieldInfo.Name).SetValue(Tmp, reader.GetDateTime(i));
                                    break;
                            }
                        }
                    }
                    clsListT.Add(Tmp);
                    Tmp = (T)Activator.CreateInstance(typeof(T));
                }
                reader.Close();
            }
            return clsListT;
        }

        /// <summary> 根據Command資料數量 </summary>
        /// <param name="conn"></param>
        /// <param name="strCommand"></param>
        /// <returns></returns>
        public static int GetCount(MySqlConnection conn, string strCommand)
        {
            int iReturn = 0;
            if (!conn.Ping()) conn.Open();
            if (conn.Ping())
            {
                using (MySqlCommand cmd = new MySqlCommand(strCommand, conn))
                {
                    MySqlDataReader reader = cmd.ExecuteReader(); //execure the reader
                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.VisibleFieldCount; i++)
                        {
                            if (reader.IsDBNull(i)) continue;
                            switch (reader.GetName(i))
                            {
                                case "count(*)":
                                    iReturn = reader.GetInt32(i);
                                    break;
                            }
                        }
                    }
                    reader.Close();
                }
            }
            return iReturn;
        }

        /// <summary> 執行Command </summary>
        /// <param name="conn"></param>
        /// <param name="strCommand"></param>
        /// <returns>成功return 1; 失敗return -1</returns>
        public static int Command(MySqlConnection conn, string strCommand)
        {
            try
            {
                if (!conn.Ping()) conn.Open();
                if (conn.Ping())
                {
                    MySqlCommand command = conn.CreateCommand();
                    command.CommandText = strCommand;
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                return -1;
            }
            return 1;
        }

        /// <summary> 上傳Xml序列化轉ByteArray至DB SettingPool </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="clsSqlConn">DB Connection</param>
        /// <param name="strKey">SettingPool Key</param>
        /// <param name="clsXmlObject">欲寫入的Xml序列化轉ByteArray物件</param>
        /// <param name="clsLog">XXLog</param>
        public static void UploadSettingPool<T>(MySqlConnection clsSqlConn, string strKey, T clsXmlObject, XXLog clsLog)
        {
            if (clsXmlObject == null) return;
            if (clsSqlConn == null) return;
            if (clsLog == null) return;

            byte[] clsDataArray = XFile.WriteXmlSerializeToArray(clsXmlObject);
            if (!clsSqlConn.Ping()) clsSqlConn.Open();
            if (clsSqlConn.Ping())
            {
                try
                {
                    String cmdText = "SELECT Count(*) FROM `SettingPool` WHERE `Key` = '" + strKey + "'";
                    MySqlCommand clsCmd = new MySqlCommand(cmdText, clsSqlConn);

                    int iCount = int.Parse(clsCmd.ExecuteScalar().ToString());
                    if (iCount > 0) clsCmd.CommandText = "UPDATE `SettingPool` SET `Data`  = @Data WHERE `Key` = '" + strKey + "'";
                    else clsCmd.CommandText = "INSERT INTO `SettingPool` (`Key`, `Data`) VALUES ('" + strKey + "', @Data)";

                    clsCmd.Parameters.Add("@Data", MySqlDbType.LongBlob);
                    clsCmd.Parameters["@Data"].Value = clsDataArray;
                    clsCmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    clsLog.Info("UploadSettingPool : " + ex.Message);
                }
            }
        }

        /// <summary> 下載DB SettingPool ByteArray 並轉成Xml序列化 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="clsSqlConn">DB Connection</param>
        /// <param name="strKey">SettingPool Key</param>
        /// <param name="clsLog">XXLog</param>
        /// <param name="clsObject">欲轉出的Xml序列化</param>
        public static void DownloadSettingPool<T>(MySqlConnection clsSqlConn, string strKey, XXLog clsLog, ref T clsObject)
        {
            if (clsObject == null) return;
            if (clsSqlConn == null) return;
            if (clsLog == null) return;

            if (!clsSqlConn.Ping()) clsSqlConn.Open();
            if (clsSqlConn.Ping())
            {
                try
                {
                    byte[] clsDataArray = null;

                    String cmdText = "SELECT `Data` FROM `SettingPool` WHERE `Key` = '" + strKey + "'";
                    MySqlCommand clsCmd = new MySqlCommand(cmdText, clsSqlConn);
                    MySqlDataReader reader = clsCmd.ExecuteReader();
                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.VisibleFieldCount; i++)
                        {
                            if (!reader.IsDBNull(i))
                            {
                                clsDataArray = new byte[reader.GetBytes(0, 0, null, 0, int.MaxValue)];
                                if (clsDataArray.Length != 0)
                                {
                                    reader.GetBytes(0, 0, clsDataArray, 0, clsDataArray.Length);
                                }
                            }
                        }
                    }
                    reader.Close();

                    XFile.ReadXmlSerialize(clsDataArray, ref clsObject);
                }
                catch (Exception ex)
                {
                    clsLog.Info("DownloadSettingPool : " + ex.Message);
                }
            }
        }
    }
}
