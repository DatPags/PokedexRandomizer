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
    Public images As List(Of BitmapImage)
    Public moveForms As List(Of String)
    Public moves As List(Of List(Of MoveInfo))
    Public abilities As List(Of List(Of String))
End Structure

Public Structure MoveInfo
    Public name As String
    Public type As String
    Public category As String
    Public power As String
    Public accuracy As String
End Structure

Public Structure UrlInfo
    Public name As String
    Public number As String
    Public url As String
End Structure

Structure UrlMapData
    Public count As Integer
    Public inInfocard As Boolean
    Public infocardDepth As Integer
    Public nextHref As Boolean
    Public urls As List(Of String)
    Public names As List(Of String)
    Public nextName As Boolean
End Structure

Structure GatherInfoData
    Public number As Integer
    Public nextName As Boolean
    Public name As String
    Public inEntryTable As Boolean
    Public checkNextEntryTable As Boolean
    Public nextNewCategory As Boolean
    Public currentEntryCategory As String
    Public inGameList As Boolean
    Public gameListDepth As Integer
    Public gamesTempStr As String
    Public nextEntry As Boolean
    Public games As List(Of String)
    Public entries As List(Of String)
    Public forms As List(Of String)
    Public classifications As List(Of String)
    Public types As List(Of List(Of String))
    Public inTabList As Boolean
    Public tabListDepth As Integer
    Public inTabTable As Boolean
    Public tabTableDepth As Integer
    Public nextDataMode As Boolean
    Public dataMode As Integer
    Public dataModeDepth As Integer
    Public tempTypes As List(Of String)
    Public firstTabList As Boolean
    Public firstTabTable As Boolean
    Public formDataFound As Boolean
    Public heights As List(Of String)
    Public weights As List(Of String)
    Public nextAbility As Boolean
    Public abilities As List(Of List(Of String))
End Structure

Structure GatherMovesData
    Public number As Integer
    Public inTabList As Boolean
    Public firstTabList As Boolean
    Public tabListDepth As Integer
    Public forms As List(Of String)
    Public formDataFound As Boolean
    Public nextName As Boolean
    Public name As String
    Public inMovesTable As Boolean
    Public checkNextMovesTable As Boolean
    Public inTabsPanelActive As Boolean
    Public tabsPanelActiveDepth As Integer
    Public firstTabsPanelActive As Boolean
    Public formMovesExist As Boolean
    Public inSmallMoveTable As Boolean
    Public smallMoveTableDepth As Integer
    Public currentForm As Integer
    Public moves As List(Of List(Of MoveInfo))
    Public nextMove As Boolean
    Public skipLeaveCheck As Boolean
    Public addMoveToAll As Boolean
    Public validForms As List(Of String)
    Public inMoveTabList As Boolean
    Public moveTabListDepth As Integer
    Public skipMoveTabCheck As Boolean
    Public moveCountdown As Integer
    Public tempMove As MoveInfo
End Structure

Module PkmnStructs
    Public Function New_UrlMapData() As UrlMapData
        Return New UrlMapData With {
            .count = 0,
            .inInfocard = False,
            .infocardDepth = 0,
            .nextHref = False,
            .urls = New List(Of String),
            .names = New List(Of String),
            .nextName = False}
    End Function

    Public Function New_GatherInfoData(pkmnNumber As Integer) As GatherInfoData
        Return New GatherInfoData With {
            .number = pkmnNumber,
            .nextName = False,
            .name = "",
            .inEntryTable = False,
            .checkNextEntryTable = False,
            .nextNewCategory = False,
            .currentEntryCategory = "",
            .inGameList = False,
            .gameListDepth = 0,
            .gamesTempStr = "",
            .nextEntry = False,
            .games = New List(Of String),
            .entries = New List(Of String),
            .forms = New List(Of String),
            .classifications = New List(Of String),
            .types = New List(Of List(Of String)),
            .inTabList = False,
            .tabListDepth = 0,
            .inTabTable = False,
            .tabTableDepth = 0,
            .nextDataMode = False,
            .dataMode = 0,
            .dataModeDepth = 0,
            .tempTypes = New List(Of String),
            .firstTabList = True,
            .firstTabTable = True,
            .formDataFound = False,
            .heights = New List(Of String),
            .weights = New List(Of String),
            .nextAbility = False,
            .abilities = New List(Of List(Of String))}
    End Function

    Public Function New_GatherMovesData(pkmnNumber As Integer) As GatherMovesData
        Return New GatherMovesData With {
            .number = pkmnNumber,
            .inTabList = False,
            .firstTabList = True,
            .tabListDepth = 0,
            .forms = New List(Of String),
            .formDataFound = False,
            .nextName = False,
            .name = "",
            .inMovesTable = False,
            .checkNextMovesTable = False,
            .inTabsPanelActive = False,
            .tabsPanelActiveDepth = 0,
            .firstTabsPanelActive = True,
            .formMovesExist = False,
            .inSmallMoveTable = False,
            .smallMoveTableDepth = 0,
            .currentForm = 0,
            .moves = New List(Of List(Of MoveInfo)),
            .nextMove = False,
            .skipLeaveCheck = False,
            .addMoveToAll = True,
            .validForms = New List(Of String),
            .inMoveTabList = False,
            .moveTabListDepth = 0,
            .skipMoveTabCheck = False,
            .moveCountdown = 0,
            .tempMove = Nothing}
    End Function

    Public Function Load_Info_Into_PkmnInfo(data As GatherInfoData) As PkmnInfo
        Return New PkmnInfo With {.number = data.number, .name = data.name, .forms = data.forms,
            .species = data.classifications, .types = data.types, .height = data.heights, .weight = data.weights,
            .games = data.games, .entries = data.entries, .images = New List(Of BitmapImage),
            .moveForms = New List(Of String), .moves = New List(Of List(Of MoveInfo)),
            .abilities = data.abilities}
    End Function
End Module
