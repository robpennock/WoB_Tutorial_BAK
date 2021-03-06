package utility;

public class Vector3<T> {

    private T x;
    private T y;
    private T z;
    // Static Variables
    public static Vector3<Float> zero = new Vector3<Float>(0.f, 0.f, 0.f);

    public Vector3() {
    }

    public Vector3(T x, T y, T z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public T getX() {
        return x;
    }

    public T setX(T x) {
        return this.x = x;
    }

    public T getY() {
        return y;
    }

    public T setY(T y) {
        return this.y = y;
    }

    public T getZ() {
        return z;
    }

    public T setZ(T z) {
        return this.z = z;
    }
}
