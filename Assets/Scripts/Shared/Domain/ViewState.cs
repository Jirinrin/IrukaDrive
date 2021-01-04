namespace Shared.Domain
{
    public abstract class ViewState
    {
        // panX gets inverted to make 'the pan amount' more intuitive to deal with
        public float panX;
        
        protected float scaleX = 1f;
        public float beatSpacing;
    }
}