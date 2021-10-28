using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

using MTF.Areas.Identity.Data;

namespace MTF.Utilities
{
    public class layoutConsts
    {
        public string NavHeight { get; set; }
        public string TableHeadAndPagingColor { get; set; }
        public string baseColor { get; set; }
        public int medianLine { get; set; }
        public string mainTitle { get; set; }
        public int bkgOffset { get; set; }
        public CommonUser user { get; set; }
        public string uId { get; set; }
        public bool IsSignedIn { get; set; }
        public string colourModel { get; set; }
        public string barsColour { get; set; }
        public bool isSU { get; set; }
        public bool isTester { get; set; }
        public int sidenavTransitionDuration { get; set; }
        public int sidenavWidth { get; set; }
        public bool reqcntnt { get; set; }
        public int serverAndClientTimeout { get; set; }
        public int serverAndClientkeepAliveInterval { get; set; }
        public string vrs { get; set; }

        public layoutConsts(CommonUser _user, bool _isSI, AuthorizationResult _isSU, bool _reqcntnt, AuthorizationResult _isTester)
        {
            NavHeight = MTF.GlobalParameters.NavbarHeight;
            TableHeadAndPagingColor = MTF.GlobalParameters.tableHeadAndPagingColor;
            baseColor = MTF.GlobalParameters._baseColor;
            medianLine = MTF.GlobalParameters._medianLine;
            mainTitle = MTF.GlobalParameters._mainTitle;
            bkgOffset = @MTF.GlobalParameters._sidenavWidth;
            sidenavTransitionDuration = MTF.GlobalParameters._sidenavTransitionDuration;
            sidenavWidth = MTF.GlobalParameters._sidenavWidth;
            vrs = GlobalParameters._appInfo.AssemblyVersion;

            user = _user;
            if (user != null)
            {
                colourModel = user.colorModel;
                barsColour = user.barsColour;
                TableHeadAndPagingColor = barsColour;
                baseColor = barsColour;
                uId = user.Id;
            }
            else
            {
                colourModel = MTF.GlobalParameters._defaultColorModel;
            }
            IsSignedIn = _isSI;
            isSU = _isSU.Succeeded;

            reqcntnt = _reqcntnt;

            isTester = _isTester.Succeeded;

            serverAndClientTimeout = MTF.GlobalParameters._serverAndClientTimeout;
            serverAndClientkeepAliveInterval = MTF.GlobalParameters._serverAndClientkeepAliveInterval;
        }
    }
}
