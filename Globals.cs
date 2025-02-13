namespace inventory_server;

public static class Globals
{
    public enum AuditType
    {
        Register = 1,
        Login = 2,
        Logout = 3,
        AddProduct = 4,
        EditProduct = 5,
        DeleteProduct = 6,
    }
}