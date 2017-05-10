//http://stackoverflow.com/questions/3699767/register-for-com-interop-vs-make-assembly-com-visible
using System;
using System.Runtime.InteropServices;
namespace SpellChecker {
  // Since the .NET Framework interface and coclass have to behave as 
  // COM objects, we have to give them guids.
  [Guid("DBE0E8C4-1C61-41f4-B6A4-4E2F353D3D05"), ComVisible(true)]
  public interface ISpellChecker {
    int PrintHi(string name);
  }

  [Guid("C6659361-1625-4746-831C-36014B146679"), ComVisible(true)]
  public class SpellCheckerImpl : ISpellChecker {
    public int PrintHi(string name) {
      Console.WriteLine("Hello, {0}!", name);
      return 33;
    }
  }
}