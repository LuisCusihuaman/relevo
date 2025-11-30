using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

public class Program
{
    public static void Main()
    {
        var token = "eyJhbGciOiJSUzI1NiIsImNhdCI6ImNsX0I3ZDRQRDExMUFBQSIsImtpZCI6Imluc18yejlNQVRhZVNlUGtIMVk1dGNobnBaVndSRVgiLCJ0eXAiOiJKV1QifQ.eyJhenAiOiJodHRwOi8vbG9jYWxob3N0OjUxNzQiLCJleHAiOjE3NjQ0NjI0NDQsImZ2YSI6WzEsLTFdLCJpYXQiOjE3NjQ0NjIzODQsImlzcyI6Imh0dHBzOi8vYXNzdXJpbmctcmVpbmRlZXItNzQuY2xlcmsuYWNjb3VudHMuZGV2IiwibmJmIjoxNzY0NDYyMzc0LCJzaWQiOiJzZXNzXzM2QXZqNjZBbFR6TUhhTGZjQlZNVDJmNWlFbyIsInN0cyI6ImFjdGl2ZSIsInN1YiI6InVzZXJfMzJHWUE2UGJ0S0k5R1dNWUlNZWJwb0I1OXBTIiwidiI6Mn0.Wj_fil5fbuU3exSm_uEUCmQUNdWlndJQ5eceGLo66KvfnjzPP_X779RLlJYS-bk6fN11Sus_FUQVk1k-ndWdjwXmPX3ka3FnhWr_3PbEgLe-ifvUL_Uh57dgbde_p2d4zjgfMh3kGS7cclbPLnjeziRxQLlaw_NaSJOuNH70LwGhIQiPtBHD3wBv8gDPR3jwwM_wOv6FwS267Zv5ev0o3jMdQaBx5LigAVDqUE163D2Ponx1az7WXs8lgnjmelEq2KB_31soPt-8JvH3LlELExqZZqq0NdE0Cfb1-jo5rc04uFr4LIirf43Xmnk7AcluvnWJtbV2B_Am1gTVEuHukQ";

        var handler = new JwtSecurityTokenHandler();
        
        if (!handler.CanReadToken(token))
        {
            Console.WriteLine("Error: Invalid JWT format.");
            return;
        }

        var jwtToken = handler.ReadJwtToken(token);

        Console.WriteLine($"Subject (ID): {jwtToken.Subject}");
        Console.WriteLine("--- Claims Found ---");
        
        foreach (var claim in jwtToken.Claims)
        {
            Console.WriteLine($"{claim.Type}: {claim.Value}");
        }

        Console.WriteLine("--- Verification ---");
        CheckClaim(jwtToken, "email");
        CheckClaim(jwtToken, "given_name");
        CheckClaim(jwtToken, "family_name");
        CheckClaim(jwtToken, "name");
        CheckClaim(jwtToken, "picture");
        CheckClaim(jwtToken, "org_role");
    }

    static void CheckClaim(JwtSecurityToken token, string type)
    {
        var exists = token.Claims.Any(c => c.Type == type);
        Console.WriteLine($"Claim '{type}': {(exists ? "✅ Found" : "❌ MISSING")}");
    }
}

