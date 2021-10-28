using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MTF.Hubs
{
    public partial class Actor
    {
        public string pageIdent { get; set; }
        public string connection { get; set; }
        public string userId { get; set; }
        public string avlMsg { get; set; }
    }

    public class reqQElem
    {
        public string userId { get; set; }
        public string avlMsg { get; set; }

        public reqQElem (Actor a)
        {
            userId = a.userId;
            avlMsg = a.avlMsg;
        }
    }

    public class PageState
    {
        public Actor owner;
        public Queue<reqQElem> requests;

        public PageState (Actor a)
        {
            owner = a;
            requests = new Queue<reqQElem>();
        }
    }
}
