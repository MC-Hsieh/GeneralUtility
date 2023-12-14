using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeneralUtility;
using System.IO; 
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GeneralUtility.IO
{
    public class EDC
    {
        public string glass_id { get; set; }
        public string group_id { get; set; }
        public string lot_id { get; set; }
        public string product_id { get; set; }
        public string pfcd { get; set; }
        public string eqp_id { get; set; }
        public string ec_code { get; set; }
        public string route_no { get; set; }
        public string route_version { get; set; }
        public string owner { get; set; }
        public string recipe_id { get; set; }
        public string operation { get; set; }
        public string rtc_flag { get; set; }
        public string pnp { get; set; }
        public string chamber { get; set; }
        public string cassette_id { get; set; }
        public string line_batch_id { get; set; }
        public string split_id { get; set; }
        public string cldate { get; set; }
        public string cltime { get; set; }
        public string mes_link_key { get; set; }
        public string rework_count { get; set; }
        public string @operator { get; set; }
        public string reserve_field_1 { get; set; }
        public string reserve_field_2 { get; set; }
        public List<iary> datas { get; set; }
    }

    public class iary 
    {
        public string item_name { get; set; }
        public string item_type { get; set; }
        public string item_value { get; set; }
    }

    public class EDC_API 
    {
        /// <summary>web server write edc</summary>
        /// <param name="strApiUrl">web server Uri</param>
        /// <param name="sEDCAddress">edc server address</param>
        /// <param name="sEqpID">站點</param>
        /// <param name="sProductID">Product ID</param>
        /// <param name="sPanelID">Panel ID</param>
        /// <param name="dictEDCnode_datas">Data</param>
        /// <param name="strDataType">Item 類別</param>
        /// <param name="bWebServerBackup">web server是否備份</param>
        /// <param name="bEDCAddressBackup">EDC Address是否備份</param>
        /// <returns></returns>
        public static string CreateEDC_webserver(string strApiUrl, string sEDCAddress, string sEqpID, string sProductID, string sPanelID,
            Dictionary<string[], string[]> dictEDCnode_datas, string strDataType = "EDC", bool bWebServerBackup = true, bool bEDCAddressBackup = true) 
        {
            List<Dictionary<string, string>> DATA = new List<Dictionary<string,string>>(); 
            foreach (var key_val in dictEDCnode_datas)
            {
                int i;
                for (i = 0; i < key_val.Key.Length; i++)
                {
                    Dictionary<string, string> dict_iary = new Dictionary<string,string>(); 
                    dict_iary.Add("item_name", key_val.Key[i]);
                    dict_iary.Add("item_type", strDataType);
                    dict_iary.Add("item_value", (i < key_val.Value.Length) ? key_val.Value[i] : "0");
                    DATA.Add(dict_iary); 
                }
            }
            IDictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("prod_id", sProductID);
            parameters.Add("panel_id", sPanelID);
            parameters.Add("station", sEqpID);
            parameters.Add("datetime", DateTime.Now.ToString("yyyyMMddHHmmss"));
            parameters.Add("server", sEDCAddress);
            parameters.Add("data", JsonConvert.SerializeObject(DATA));

            JObject json = GeneralUtility.Net.WebServer.POST_WebResopsne_Json(strApiUrl, parameters, Encoding.UTF8); 

            if (json == null)
            {
                return "Exception NG"; 
            }
            else
            {
                if (json["result"].ToString() == "OK")
                {
                    return "OK"; 
                }
                else
                {
                    return "NG|" + json["message"].ToString();
                }
            }
        }

        public static void CreateEDC_Local(string sEDCPath, string sEqpID, string sProductID, string sPanelID, Dictionary<string[], string[]> dictEDCnode_datas, string strDataType = "EDC") 
        {
            string[] path = new string[] { sEDCPath };
            CreateEDC_Local(path, sEqpID, sProductID, sPanelID, dictEDCnode_datas, strDataType); 
        }

        public static void CreateEDC_Local(string[] sEDCPaths, string sEqpID, string sProductID, string sPanelID, Dictionary<string[], string[]> dictEDCnode_datas, string strDataType = "EDC")
        {
            ///建立EDC
            EDC buff_EDC = new EDC();

            ///寫入相關資料
            buff_EDC.glass_id = sPanelID;
            buff_EDC.group_id = "";
            buff_EDC.lot_id = "";
            buff_EDC.product_id = sProductID;
            buff_EDC.pfcd = sProductID;
            buff_EDC.eqp_id = sEqpID;
            buff_EDC.ec_code = "";
            buff_EDC.route_no = "";
            buff_EDC.route_version = "";
            buff_EDC.owner = "";
            buff_EDC.recipe_id = "";
            buff_EDC.operation = "";
            buff_EDC.rtc_flag = "";
            buff_EDC.pnp = "";
            buff_EDC.chamber = "";
            buff_EDC.cassette_id = "";
            buff_EDC.line_batch_id = "";
            buff_EDC.split_id = "";
            buff_EDC.cldate = DateTime.Now.ToString("yyyy-MM-dd");
            buff_EDC.cltime = DateTime.Now.ToString("hh:mm:ss");
            buff_EDC.mes_link_key = "";
            buff_EDC.rework_count = "";
            buff_EDC.@operator = "";
            buff_EDC.reserve_field_1 = "";
            buff_EDC.reserve_field_2 = "";

            ///初始化數據整列
            buff_EDC.datas = new List<iary>();

            foreach (var key_val in dictEDCnode_datas)
            {
                int i;
                for (i = 0; i < key_val.Key.Length; i++)
                {
                    iary item = new iary();
                    item.item_name = key_val.Key[i];
                    item.item_type = strDataType;
                    item.item_value = (i < key_val.Value.Length) ? key_val.Value[i] : "0";
                    buff_EDC.datas.Add(item);
                }
            }

            foreach (string path in sEDCPaths)
            {
                ///檢查資料夾路徑
                string sEdcDir = path + sEqpID + @"\";

                if (!Directory.Exists(sEdcDir))
                {
                    Directory.CreateDirectory(sEdcDir);
                }

                ///生成檔案完整路徑
                string sSavePath = sEdcDir + DateTime.Now.ToString("yyyyMMddhhmmss") + "_" + sEqpID + "_" + sPanelID + ".xml";

                ///寫入檔案
                GeneralUtility.IO.XFile.WriteXmlSerialize(sSavePath, buff_EDC);
            }
        }
    }
}
