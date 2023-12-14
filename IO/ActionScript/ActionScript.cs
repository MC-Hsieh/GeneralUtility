using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.Threading;

namespace GeneralUtility.IO.ActionScript
{
    [Serializable]
    public class ActionScript
    {
        public const char CSPLIT = '|';

        public List<ActionUnit> ActionUnitList
        {
            set { _ActionUnitList = value; }
            get { return _ActionUnitList; }
        }
        private List<ActionUnit> _ActionUnitList;

        public void AddUnit(ActionUnit clsActionUnit)
        {
            if (_ActionUnitList == null) _ActionUnitList = new List<ActionUnit>();
            _ActionUnitList.Add(clsActionUnit);
        }

        public void RemoveUnit(ActionUnit clsActionUnit)
        {
            if (_ActionUnitList == null) _ActionUnitList = new List<ActionUnit>();
            if (_ActionUnitList.Contains(clsActionUnit))
            {
                _ActionUnitList.Remove(clsActionUnit);
            }
        }

        /// <summary>執行動作腳本</summary>
        /// <param name="iDelay">單元間隔(ms)</param>
        public void Action(int iDelay = 0)
        {
            if (_ActionUnitList != null)
            {
                for (int i = 0; i < _ActionUnitList.Count; i++)
                {
                    _ActionUnitList[i].Action();
                    if (iDelay != 0)
                    {
                        Thread.Sleep(iDelay);
                    }
                }
            }
        }

    }
}
