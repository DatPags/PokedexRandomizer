﻿Imports SixLabors.ImageSharp

Public Class PkmnImageFinderPokesprite
    Implements IPkmnImageFinder

    Private Const URL_IMG_PRE = "https://raw.githubusercontent.com/msikma/pokesprite/master/pokemon-gen8/regular/"
    Private Const URL_IMG_FEMALE = "female/"
    Private Const URL_IMG_POST = ".png"
    Private Const URL_IMG_UNKNOWN = "https://raw.githubusercontent.com/msikma/pokesprite/master/pokemon-gen8/unknown.png"
    Private Const URL_IMG_JSON = "https://raw.githubusercontent.com/msikma/pokesprite/master/data/pokemon.json"

    Private Const URL_IMG_PHYS = "https://img.pokemondb.net/images/icons/physical.png"
    Private Const URL_IMG_SPEC = "https://img.pokemondb.net/images/icons/special.png"
    Private Const URL_IMG_STAT = "https://img.pokemondb.net/images/icons/status.png"

    Private _imageJson As Newtonsoft.Json.Linq.JObject

    Private _settings As Settings
    Private _cache As IImageCache

    Public ReadOnly Property SupportsRapidLookup As Boolean Implements IPkmnImageFinder.SupportsRapidLookup
        Get
            Return False
        End Get
    End Property

    Public Shared Async Function CreateSelfAsync(settings As Settings, Optional cache As IImageCache = Nothing) As Task(Of PkmnImageFinderPokesprite)
        Dim obj = New PkmnImageFinderPokesprite(settings, cache)
        Dim text = Await UtilWeb.GetTextFromUrlAsync(URL_IMG_JSON)
        obj._imageJson = Newtonsoft.Json.Linq.JObject.Parse(text)
        Return obj
    End Function

    Public Sub New(settings As Settings, cache As IImageCache)
        _settings = settings
        _cache = cache
    End Sub

    Public Async Function GetPkmnIconListAsync(pkmnInfo As PkmnInfo) As Task(Of List(Of Image)) Implements IPkmnImageFinder.GetPkmnIconListAsync
        Return Await GetPkmnImageListAsync(pkmnInfo)
    End Function

    Public Async Function GetPkmnImageListAsync(pkmnInfo As PkmnInfo) As Task(Of List(Of Image)) Implements IPkmnImageFinder.GetPkmnImageListAsync
        Dim imgList As New List(Of Image), baseName As String = "", formsToken As Newtonsoft.Json.Linq.JToken = Nothing, forms As New List(Of Newtonsoft.Json.Linq.JToken)
        Dim imgUnknown As Boolean = False
        Try
            imgList = New List(Of Image)
            baseName = _imageJson.SelectToken(pkmnInfo.number.ToString("D3")).SelectToken("slug").SelectToken("eng").ToString
            formsToken = _imageJson.SelectToken(pkmnInfo.number.ToString("D3")).SelectToken("gen-8").SelectToken("forms")
            forms = formsToken.Children.ToList
        Catch ex As Exception
            ' Pokedex number is not in the list
            imgUnknown = True
        End Try

        For formIndex = 0 To pkmnInfo.forms.Count - 1
            ' Unknown image for Pokemon not added yet
            If imgUnknown Then
                If imgList.Count = 0 Then
                    Try
                        imgList.Add(Await GetUnknownImageAsync())
                    Catch ex As System.Net.WebException
                        imgList.Add(Nothing)
                    End Try
                Else
                    imgList.Add(imgList(0))
                End If
                Continue For
            End If

            ' Special cases for certain Pokemon
            If pkmnInfo.name = "Urshifu" Then
                Try
                    imgList.Add(Await GetPkmnImageAsync(baseName))
                Catch ex As System.Net.WebException
                    imgList.Add(Nothing)
                End Try
                Continue For
            ElseIf pkmnInfo.name = "Pikachu" AndAlso formIndex > 0 Then
                imgList.Add(imgList(0))
                Continue For
            ElseIf pkmnInfo.name = "Minior" AndAlso formIndex > 0 Then
                Try
                    imgList.Add(Await GetPkmnImageAsync(baseName & "-blue"))
                Catch ex As System.Net.WebException
                    imgList.Add(Nothing)
                End Try
                Continue For
            ElseIf pkmnInfo.name = "Darmanitan" AndAlso pkmnInfo.forms(formIndex) = "Galarian Zen Mode" Then
                Try
                    imgList.Add(Await GetPkmnImageAsync(baseName & "-galar-zen"))
                Catch ex As System.Net.WebException
                    imgList.Add(Nothing)
                End Try
                Continue For
            End If

            If formIndex = 0 AndAlso pkmnInfo.name = pkmnInfo.forms(formIndex) Then
                ' Get base image if this is first form and form name = pokemon name
                Try
                    imgList.Add(Await GetPkmnImageAsync(baseName))
                Catch ex As System.Net.WebException
                    imgList.Add(Nothing)
                End Try
            Else
                ' For male/female form differences, check in a different location if they exist
                If pkmnInfo.forms(formIndex) = "Male" Then
                    Try
                        imgList.Add(Await GetPkmnImageAsync(baseName))
                    Catch ex As System.Net.WebException
                        If imgList.Count = 0 Then
                            imgList.Add(Nothing)
                        Else
                            imgList.Add(imgList(0))
                        End If
                    End Try
                    Continue For
                ElseIf pkmnInfo.forms(formIndex) = "Female" Then
                    Dim getFemaleImg = False
                    Dim baseForm = formsToken.Children.ToList(0).SelectToken("$")
                    Dim hasFemaleToken = baseForm.Children.ToList(0).SelectToken("has_female")
                    If hasFemaleToken IsNot Nothing Then
                        Dim hasFemale = Convert.ToBoolean(hasFemaleToken)
                        If hasFemale Then
                            Dim unofficialToken = baseForm.SelectToken("has_unofficial_female_icon")
                            If unofficialToken IsNot Nothing Then
                                Dim unofficial = Convert.ToBoolean(unofficialToken)
                                If Not unofficial Then
                                    getFemaleImg = True
                                End If
                            Else
                                getFemaleImg = True
                            End If
                        End If
                    End If
                    If getFemaleImg Then
                        Try
                            imgList.Add(Await GetPkmnImageAsync(URL_IMG_FEMALE & baseName))
                        Catch ex As System.Net.WebException
                            If imgList.Count = 0 Then
                                imgList.Add(Nothing)
                            Else
                                imgList.Add(imgList(0))
                            End If
                        End Try
                    Else
                        Try
                            imgList.Add(Await GetPkmnImageAsync(baseName))
                        Catch ex As System.Net.WebException
                            If imgList.Count = 0 Then
                                imgList.Add(Nothing)
                            Else
                                imgList.Add(imgList(0))
                            End If
                        End Try
                    End If
                    Continue For
                End If

                ' Reduce form name into a version that we can check if it's a form image name
                Dim formName = System.Text.RegularExpressions.Regex.Replace(pkmnInfo.forms(formIndex), "[^a-zA-Z0-9- ]", "")
                Dim pkmnNameFix = System.Text.RegularExpressions.Regex.Replace(pkmnInfo.name, "[^a-zA-Z0-9- ]", "")
                Dim imgNameCheck As String
                If formName.Contains(pkmnNameFix) Then
                    imgNameCheck = formName.Remove(formName.IndexOf(pkmnNameFix), pkmnNameFix.Length)
                    imgNameCheck = imgNameCheck.Trim.ToLower.Replace("  ", " ").Replace(" ", "-")
                Else
                    Dim formNameParts = formName.ToLower.Split(" ").ToList
                    If formNameParts.Count <= 1 Then
                        imgNameCheck = formName.ToLower
                    Else
                        formNameParts.RemoveAt(formNameParts.Count - 1)
                        imgNameCheck = String.Join("-", formNameParts)
                    End If
                End If

                ' Look through the json for a form name match
                Dim match = False
                Dim useDefault = False
                For Each imgForm In forms
                    Dim imgFormProp = CType(imgForm, Newtonsoft.Json.Linq.JProperty)
                    If Not imgFormProp.Name = "$" And (imgFormProp.Name = imgNameCheck Or imgFormProp.Name.StartsWith(imgNameCheck) Or imgNameCheck.StartsWith(imgFormProp.Name)) Then
                        imgNameCheck = imgFormProp.Name
                        Dim props = imgForm.Children.ToList(0)
                        For Each tempProp In props
                            Dim prop = CType(tempProp, Newtonsoft.Json.Linq.JProperty)
                            If prop.Name = "is_alias_of" Then
                                If prop.Value.ToString = "$" Then
                                    useDefault = True
                                Else
                                    imgNameCheck = prop.Value.ToString
                                End If
                                Exit For
                            End If
                        Next
                        match = True
                        Exit For
                    End If
                Next

                ' Get the proper image if there was a match or not
                If match Then
                    Try
                        If useDefault Then
                            imgList.Add(Await GetPkmnImageAsync(baseName))
                        Else
                            imgList.Add(Await GetPkmnImageAsync(baseName & "-" & imgNameCheck))
                        End If
                    Catch ex As System.Net.WebException
                        If imgList.Count = 0 Then
                            imgList.Add(Nothing)
                        Else
                            imgList.Add(imgList(0))
                        End If
                    End Try
                Else
                    If imgList.Count = 0 Then
                        Dim fail As Boolean = False
                        Try
                            imgList.Add(Await GetPkmnImageAsync(baseName))
                        Catch ex As System.Net.WebException
                            fail = True
                        End Try

                        If fail Then
                            Try
                                imgList.Add(Await GetUnknownImageAsync())
                            Catch ex As System.Net.WebException
                                imgList.Add(Nothing)
                            End Try
                        End If
                    Else
                        Try
                            imgList.Add(Await GetUnknownImageAsync())
                        Catch ex As System.Net.WebException
                            imgList.Add(imgList(0))
                        End Try
                    End If
                End If
            End If
        Next
        Return imgList
    End Function

    Private Async Function GetPkmnImageAsync(imgName As String) As Task(Of Image)
        Return Await UtilImage.GetImageFromUrlAsync(URL_IMG_PRE & imgName & URL_IMG_POST, _settings, _cache)
    End Function

    Private Async Function GetUnknownImageAsync() As Task(Of Image)
        Return Await UtilImage.GetImageFromUrlAsync(URL_IMG_UNKNOWN, _settings, _cache)
    End Function

    Public Async Function GetMoveCategoryImageAsync(category As String) As Task(Of Image) Implements IPkmnImageFinder.GetMoveCategoryImageAsync
        Select Case category.ToLower
            Case "physical"
                Return Await UtilImage.GetImageFromUrlAsync(URL_IMG_PHYS, _settings, _cache)
            Case "special"
                Return Await UtilImage.GetImageFromUrlAsync(URL_IMG_SPEC, _settings, _cache)
            Case "status"
                Return Await UtilImage.GetImageFromUrlAsync(URL_IMG_STAT, _settings, _cache)
            Case Else
                Return Nothing
        End Select
    End Function
End Class
