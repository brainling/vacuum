using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vacuum.Core.Ui {
    public interface IFlyoutViewModel {
        void Closing ();
        void Closed ();
    }
}
