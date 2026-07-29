using System;
using System.Text.RegularExpressions;

namespace CRM.WebFrontend.Client.Helpers;

public static class ValidationHelper
{
    public static bool ValidateDni(string dni)
    {
        if (string.IsNullOrWhiteSpace(dni)) return false;
        
        // Clean DNI string
        dni = dni.Trim().ToUpper();
        
        // Basic pattern: 8 digits + 1 letter, or X/Y/Z + 7 digits + 1 letter (NIE)
        var pattern = new Regex(@"^([0-9]{8}[TRWAGMYFPDXBNJZSQVHLCKE]|[XYZ][0-9]{7}[TRWAGMYFPDXBNJZSQVHLCKE])$");
        if (!pattern.IsMatch(dni)) return false;
        
        try
        {
            string numberPart = dni;
            
            // Handle NIE prefix conversion: X=0, Y=1, Z=2
            if (dni.StartsWith("X"))
                numberPart = "0" + dni.Substring(1);
            else if (dni.StartsWith("Y"))
                numberPart = "1" + dni.Substring(1);
            else if (dni.StartsWith("Z"))
                numberPart = "2" + dni.Substring(1);
                
            var number = int.Parse(numberPart.Substring(0, 8));
            var letter = dni[^1];
            
            const string lettersMap = "TRWAGMYFPDXBNJZSQVHLCKE";
            var calculatedLetter = lettersMap[number % 23];
            
            return letter == calculatedLetter;
        }
        catch
        {
            return false;
        }
    }

    public static bool ValidateIban(string iban)
    {
        if (string.IsNullOrWhiteSpace(iban)) return false;
        
        // Clean string: remove spaces and dashes, convert to uppercase
        iban = iban.Replace(" ", "").Replace("-", "").Trim().ToUpper();
        
        // General IBAN format check (Spain starts with ES + 22 digits)
        if (!Regex.IsMatch(iban, @"^ES\d{22}$")) return false;
        
        try
        {
            // Move first 4 chars to the end (ESxx -> end)
            string rearranged = iban.Substring(4) + iban.Substring(0, 4);
            
            // Replace letters with numbers (A=10, B=11... Z=35)
            // E = 14, S = 28
            var numericString = "";
            foreach (var c in rearranged)
            {
                if (char.IsLetter(c))
                {
                    numericString += (c - 'A' + 10).ToString();
                }
                else
                {
                    numericString += c;
                }
            }
            
            // Calculate modulo 97 using big integer logic or custom mod97 division
            return Modulo97(numericString) == 1;
        }
        catch
        {
            return false;
        }
    }

    private static int Modulo97(string numericStr)
    {
        // Compute mod 97 of a very large numeric string
        var checksum = 0;
        for (int i = 0; i < numericStr.Length; i += 7)
        {
            var segment = checksum.ToString() + numericStr.Substring(i, Math.Min(7, numericStr.Length - i));
            checksum = int.Parse(segment) % 97;
        }
        return checksum;
    }

    public static bool ValidatePhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return false;
        
        // Clean spaces
        phone = phone.Replace(" ", "").Trim();
        
        // Spanish phone format: starts with 6, 7, 8 or 9, followed by 8 digits
        return Regex.IsMatch(phone, @"^[6789]\d{8}$");
    }
}
