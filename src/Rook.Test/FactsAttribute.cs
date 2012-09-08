using System;
using Xunit;

namespace Rook
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class FactsAttribute : RunWithAttribute
    {
        public FactsAttribute()
            :base(typeof(FactsTestClassCommand)) { }
    }
}