using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AspectWeaver.Tests {
  interface INouNou {
    int Hop();
  }
  interface IZoZo: INouNou {
    Task<int> Bof();
    Task LaLa();
  }

  class ZoZo: IZoZo {
    public int failures = 2;

    public async Task LaLa() {
      await Bof();
    }

    public async Task<int> Bof() {
      await Task.Delay(1);
      return Hop() + 1;
    }
    public int Hop() {
      if( 0 < failures-- ) {
        throw new IndexOutOfRangeException("not tried often enough");
      }
      return 666;
    }
  }
}
