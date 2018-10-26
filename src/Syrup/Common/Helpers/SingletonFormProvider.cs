using System;
using System.Windows.Forms;

namespace Syrup.Common.Helpers
{
    public class SingletonFormProvider<TForm> : IDisposable
        where TForm : Form
    {
        private TForm _currentInstance;
        private Func<TForm> _createForm;

        public SingletonFormProvider(Func<TForm> createForm)
        {
            _createForm = createForm;
        }

        public TForm CurrentInstance
        {
            get
            {
                if (_currentInstance == null)
                    _currentInstance = _createForm();

                // TODO here: wire into _currentInstance close event
                // to null _currentInstance field

                return _currentInstance;
            }
        }

        public void Close()
        {
            if (_currentInstance == null) return;

            _currentInstance.Dispose();
            _currentInstance = null;
        }

        public void Dispose()
        {
            Close();
        }
    }
}