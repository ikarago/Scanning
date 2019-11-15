using System;

using Windows.UI.Input.Inking;

namespace FluentScanner.EventHandlers.Ink
{
    public class AddStrokeEventArgs : EventArgs
    {
        public InkStroke OldStroke { get; set; }

        public InkStroke NewStroke { get; set; }

        public AddStrokeEventArgs(InkStroke newStroke, InkStroke oldStroke = null)
        {
            NewStroke = newStroke;
            OldStroke = oldStroke;
        }
    }
}
