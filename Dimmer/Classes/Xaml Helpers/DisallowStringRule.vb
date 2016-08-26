Namespace ValidationRules

    Friend Class DisallowStringRule
        Inherits ValidationRule

        Public Enum ValidationMethods
            Contains = 0
            Equals = 1
        End Enum

        Public Property InvalidString As String
        Public Property ValidationMethod As ValidationMethods

        Public Overloads Overrides Function Validate(value As Object, cultureInfo As Globalization.CultureInfo) As ValidationResult
            If (value IsNot Nothing) AndAlso
                ((ValidationMethod = ValidationMethods.Contains AndAlso value.Contains(InvalidString)) OrElse
                (ValidationMethod = ValidationMethods.Equals AndAlso value = InvalidString)) Then
                Return New ValidationResult(False, String.Format("Value '{0}' is not valid.", InvalidString))
            Else
                Return ValidationResult.ValidResult
            End If
        End Function

    End Class

End Namespace