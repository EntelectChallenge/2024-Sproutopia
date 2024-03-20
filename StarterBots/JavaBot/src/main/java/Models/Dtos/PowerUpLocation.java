package Models.Dtos;

public class PowerUpLocation {
    public Location location;
    public int type;

    public PowerUpLocation(Location location, int type) {
        this.location = location;
        this.type = type;
    }

    public Location getLocation() {
        return location;
    }

    public int getType() {
        return type;
    }

    public void setLocation(Location location) {
        this.location = location;
    }

    public void setType(int type) {
        this.type = type;
    }
}
