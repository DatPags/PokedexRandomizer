Public Interface IPkmnInfoFinder
    Function Total_Number_Of_Pokemon() As Integer
    Function Pkmn_Exists_By_Name(name As String) As Boolean
    Function Pkmn_Exists_By_Num(num As Integer) As Boolean
    Function Pkmn_Name_To_Number(name As String) As Integer
    Function Get_Pkmn_Info(num As Integer) As Task(Of PkmnInfo)
End Interface
