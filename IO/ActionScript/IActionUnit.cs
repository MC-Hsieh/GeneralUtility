using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeneralUtility.IO.ActionScript
{
    public interface IActionUnit
    {
        void Action();

        bool StringToUnit(string[] strData);

        string[] UnitToString();

        string GetDescription();
    }
}
