namespace Starter_NET_7.Helpers.Validations
{
  public static class UrlValidation
  {

    public static bool IsValidUrl(string url)
    {
      Uri validatedUri;

      if (Uri.TryCreate(url, UriKind.Absolute, out validatedUri!)) //.NET URI validation.
      {
        //If true: validatedUri contains a valid Uri. Check for the scheme in addition.
        return (validatedUri.Scheme == Uri.UriSchemeHttp || validatedUri.Scheme == Uri.UriSchemeHttps);
      }
      return false;
    }
  }
}
