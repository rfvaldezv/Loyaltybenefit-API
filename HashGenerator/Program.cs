using BCrypt.Net;

string password = "Admin123!";
// Usamos la ruta completa de la clase para evitar el error de namespace
string hash = BCrypt.Net.BCrypt.HashPassword(password); 
Console.WriteLine("Hash generado:");
Console.WriteLine(hash);
