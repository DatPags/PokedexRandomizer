Imports SixLabors.ImageSharp

Public Structure Pkmn
    Public pkmn As PkmnInfo
    Public icons As List(Of Image)
    Public iconUris As List(Of String)
    Public images As List(Of Image)
    Public imageUris As List(Of String)
End Structure

<Serializable()>
Public Structure PkmnInfo
    Public number As Integer
    Public name As String
    Public forms As List(Of String)
    Public species As List(Of String)
    Public types As List(Of List(Of String))
    Public height As List(Of String)
    Public weight As List(Of String)
    Public games As List(Of String)
    Public entries As List(Of String)
    Public moveForms As List(Of String)
    Public moves As List(Of List(Of MoveInfo))
    Public abilities As List(Of List(Of String))
End Structure

<Serializable()>
Public Structure MoveInfo
    Public name As String
    Public type As String
    Public category As String
    Public power As String
    Public accuracy As String
End Structure
