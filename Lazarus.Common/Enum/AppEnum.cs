using System;
using System.Collections.Generic;
using System.Text;

namespace Lazarus.Common.Enum
{
    public enum SourceOrderType
    {
        Other = 0,
        SAP47 = 1,

        /// <summary>
        /// SAP Hana CBM
        /// </summary>
        SAP_DSP = 2,

        /// <summary>
        /// SAP Ceramic
        /// </summary>
        SAP_TEP = 3,

        SGI = 4,
        COAL = 5,

        /// <summary>
        /// CPAC
        /// </summary>
        PFA = 6,
        /// <summary>
        /// CCS/LHP
        /// </summary>
        SSP = 7
    }

    public enum OrderType
    {
        SO,
        STO,
        RO,
        DNO
    }
}
