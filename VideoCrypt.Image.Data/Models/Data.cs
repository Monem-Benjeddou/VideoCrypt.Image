namespace VideoCrypt.Image.Data.Models;

public class Data
{
    public string id { get; set; }
    public string user_id { get; set; }
    public DateTime created { get; set; }
    public string file_name { get; set; }
    public object expires_at { get; set; }
    public string delete_key { get; set; }
}