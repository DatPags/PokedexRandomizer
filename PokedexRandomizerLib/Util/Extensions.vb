Imports System.Runtime.CompilerServices
Imports System.Security.Cryptography
Imports System.Text

Module Extensions
    <Extension()>
    Public Function GetHashCodeDeterministic(str As String) As Integer
        Return BitConverter.ToInt32(MD5.HashData(Encoding.UTF8.GetBytes(str)))
    End Function
End Module
