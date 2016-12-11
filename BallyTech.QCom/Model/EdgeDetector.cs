using System;

namespace BallyTech.QCom.Model
{
    internal class EdgeDetector<T>
    {
        private T _Old;
        private T _New;

        internal EdgeDetector(T old, T @new)
        {
            _Old = old;
            _New = @new;
        }

        internal bool Rising(Func<T, bool> flag)
        {
            if(_Old == null) return false;
            return !flag(_Old) && flag(_New);
        }

        internal bool Falling(Func<T, bool> flag)
        {
            if (_Old == null) return false;
            return flag(_Old) && !flag(_New);
        }

        internal bool Rising()
        {
            if (_Old == null) return false;
            return Rising(x => Convert.ToBoolean(x));
        }

        internal bool Falling()
        {
            if (_Old == null) return false;
            return Falling(x => Convert.ToBoolean(x));
        }


    }
}