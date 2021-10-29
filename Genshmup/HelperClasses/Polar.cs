namespace Genshmup.HelperClasses
{
    internal struct Polar
    {
        public float radius;
        public float angle;

        public Polar()
        {
            radius = 1;
            angle = 0;
        }
        public Polar(float radius, float angle)
        {
            this.radius = radius;
            this.angle = angle;
        }
    }
}
