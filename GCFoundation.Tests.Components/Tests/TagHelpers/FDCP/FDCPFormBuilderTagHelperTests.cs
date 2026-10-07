using GCFoundation.Components.Models.FormBuilder;
using GCFoundation.Components.TagHelpers.FDCP;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GCFoundation.Tests.Components.Tests.TagHelpers.FDCP
{
    public class FDCPFormBuilderTagHelperTests
    {
        [Fact]
        public void Process_WithBasicForm_RendersExpectedStructure()
        {
            // Arrange
            var tagHelper = new FDCPFormBuilderTagHelper
            {
                Form = new FormDefinition
                {
                    Id = "testForm",
                    Title = "Test Form",
                    Action = "/submit",
                    Method = "post",
                    SubmitButtonText = "Submit",
                    Sections = new[]
                    {
                        new FormSection
                        {
                            Title = "Personal Information",
                            Hint = "Please provide your details",
                            Questions = new[]
                            {
                                new FormQuestion
                                {
                                    Id = "name",
                                    Label = "Full Name",
                                    Type = QuestionType.Text,
                                    IsRequired = true
                                }
                            }
                        }
                    }
                }
            };

            var context = new TagHelperContext(
                new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                "test");

            var output = new TagHelperOutput("fdcp-form-builder",
                new TagHelperAttributeList(),
                (result, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

            // Act
            tagHelper.Process(context, output);

            // Assert
            Assert.Equal("div", output.TagName);
            Assert.Equal("gc-form", output.Attributes["class"].Value);

            var content = output.Content.GetContent();
            
            // Verify error summary is present
            Assert.Contains("<gcds-error-summary", content);
            
            // Just verify that the form was created
            Assert.Contains("<form", content);
            Assert.Contains("action='/submit'", content);
            Assert.Contains("method='post'", content);
            Assert.Contains("class='gc-form'", content);
            Assert.Contains("<gcds-fieldset", content);
            Assert.Contains("legend='Personal Information'", content);
            Assert.Contains("hint='Please provide your details'", content);
            Assert.Contains("required", content);
            Assert.Contains("Submit", content);
        }

        [Fact]
        public void Process_WithValidationRules_RendersValidationAttributes()
        {
            // Arrange
            var tagHelper = new FDCPFormBuilderTagHelper
            {
                Form = new FormDefinition
                {
                    Id = "testForm",
                    Title = "Test Form",
                    Action = "/submit",
                    Method = "post",
                    SubmitButtonText = "Submit",
                    Sections = new[]
                    {
                        new FormSection
                        {
                            Title = "Test Section",
                            Questions = new[]
                            {
                                new FormQuestion
                                {
                                    Id = "email",
                                    Label = "Email",
                                    Type = QuestionType.Email,
                                    IsRequired = true,
                                    ValidateOnBlur = true,
                                    ValidationRules = new[]
                                    {
                                        new ValidationRule
                                        {
                                            Type = ValidationRuleType.Email
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var context = new TagHelperContext(
                new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                "test");

            var output = new TagHelperOutput("fdcp-form-builder",
                new TagHelperAttributeList(),
                (result, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

            // Act
            tagHelper.Process(context, output);

            // Assert
            var content = output.Content.GetContent();
            
            // Check for the basic structure we expect
            Assert.Contains("type='email'", content);
            Assert.Contains("input-id='email'", content); // Verify required GCDS attribute
            Assert.Contains("name='email'", content); // Required GCDS attribute
        }

        [Fact]
        public void Process_WithDependencies_RendersDependencyAttributes()
        {
            // Arrange
            var tagHelper = new FDCPFormBuilderTagHelper
            {
                Form = new FormDefinition
                {
                    Id = "testForm",
                    Title = "Test Form",
                    Action = "/submit",
                    Method = "post",
                    SubmitButtonText = "Submit",
                    Sections = new[]
                    {
                        new FormSection
                        {
                            Title = "Test Section",
                            Questions = new[]
                            {
                                new FormQuestion
                                {
                                    Id = "question2",
                                    Label = "Dependent Question",
                                    Type = QuestionType.Text,
                                    Dependencies = new[]
                                    {
                                        new QuestionDependency
                                        {
                                            SourceQuestionId = "question1",
                                            TargetQuestionId = "question2",
                                            TriggerValue = "yes",
                                            Action = DependencyAction.Show
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var context = new TagHelperContext(
                new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                "test");

            var output = new TagHelperOutput("fdcp-form-builder",
                new TagHelperAttributeList(),
                (result, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

            // Act
            tagHelper.Process(context, output);

            // Assert
            var content = output.Content.GetContent();
            Assert.Contains("data-dependencies", content);
        }

        [Fact]
        public void Process_NullOutput_ThrowsArgumentNullException()
        {
            // Arrange
            var tagHelper = new FDCPFormBuilderTagHelper
            {
                Form = new FormDefinition
                {
                    Id = "testForm",
                    Title = "Test Form",
                    Action = "/submit",
                    Method = "post",
                    SubmitButtonText = "Submit",
                    Sections = Array.Empty<FormSection>()
                }
            };

            var context = new TagHelperContext(
                new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                "test");

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => tagHelper.Process(context, null!));
        }

        [Fact]
        public void Process_WithRequiredInputs_IncludesGCDSRequiredAttributes()
        {
            // Arrange
            var tagHelper = new FDCPFormBuilderTagHelper
            {
                Form = new FormDefinition
                {
                    Id = "testForm",
                    Title = "Test Form",
                    Action = "/submit",
                    Method = "post",
                    SubmitButtonText = "Submit",
                    Sections = new[]
                    {
                        new FormSection
                        {
                            Title = "Test Section",
                            Questions = new[]
                            {
                                new FormQuestion
                                {
                                    Id = "testInput",
                                    Label = "Test Input",
                                    Type = QuestionType.Text,
                                    IsRequired = true,
                                    Size = 25,
                                    ErrorMessage = "This field is required"
                                }
                            }
                        }
                    }
                }
            };

            var context = new TagHelperContext(
                new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                "test");

            var output = new TagHelperOutput("fdcp-form-builder",
                new TagHelperAttributeList(),
                (result, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

            // Act
            tagHelper.Process(context, output);

            // Assert - Verify GCDS required attributes
            var content = output.Content.GetContent();
            Assert.Contains("name='testInput'", content); // Required by GCDS
            Assert.Contains("input-id='testInput'", content); // Required by GCDS
            Assert.Contains("label='Test Input'", content); // Required by GCDS
            Assert.Contains("size='25'", content); // Size attribute support
            Assert.Contains("error-message=\"This field is required\"", content); // Error message support
            Assert.Contains("validate-on=\"blur\"", content); // Default validation event
        }

        [Fact]
        public void Process_WithTextArea_RendersCorrectGCDSComponent()
        {
            // Arrange
            var tagHelper = new FDCPFormBuilderTagHelper
            {
                Form = new FormDefinition
                {
                    Id = "testForm",
                    Title = "Test Form",
                    Action = "/submit",
                    Method = "post",
                    SubmitButtonText = "Submit",
                    Sections = new[]
                    {
                        new FormSection
                        {
                            Title = "Test Section",
                            Questions = new[]
                            {
                                new FormQuestion
                                {
                                    Id = "comments",
                                    Label = "Comments",
                                    Type = QuestionType.TextArea,
                                    Size = 5,
                                    Value = "Initial value"
                                }
                            }
                        }
                    }
                }
            };

            var context = new TagHelperContext(
                new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                "test");

            var output = new TagHelperOutput("fdcp-form-builder",
                new TagHelperAttributeList(),
                (result, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

            // Act
            tagHelper.Process(context, output);

            // Assert
            var content = output.Content.GetContent();
            Assert.Contains("<gcds-textarea", content);
            Assert.Contains("textarea-id='comments'", content);
            Assert.Contains("rows='5'", content);
            Assert.Contains("Initial value", content);
        }

        [Fact]
        public void Process_WithTextAreaMaxLengthFromProperty_RendersMaxlengthAttribute()
        {
            var tagHelper = new FDCPFormBuilderTagHelper
            {
                Form = new FormDefinition
                {
                    Id = "testForm",
                    Title = "Test Form",
                    Action = "/submit",
                    Method = "post",
                    SubmitButtonText = "Submit",
                    Sections = new[]
                    {
                        new FormSection
                        {
                            Title = "Test Section",
                            Questions = new[]
                            {
                                new FormQuestion
                                {
                                    Id = "comments",
                                    Label = "Comments",
                                    Type = QuestionType.TextArea,
                                    MaxLength = 500
                                }
                            }
                        }
                    }
                }
            };

            var context = new TagHelperContext(
                new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                "test");

            var output = new TagHelperOutput("fdcp-form-builder",
                new TagHelperAttributeList(),
                (result, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

            tagHelper.Process(context, output);

            var content = output.Content.GetContent();
            Assert.Contains("maxlength='500'", content);
            Assert.DoesNotContain("character-count", content);
        }

        [Fact]
        public void Process_WithTextAreaMaxLengthFromValidationRule_RendersMaxlengthAttribute()
        {
            var tagHelper = new FDCPFormBuilderTagHelper
            {
                Form = new FormDefinition
                {
                    Id = "testForm",
                    Title = "Test Form",
                    Action = "/submit",
                    Method = "post",
                    SubmitButtonText = "Submit",
                    Sections = new[]
                    {
                        new FormSection
                        {
                            Title = "Test Section",
                            Questions = new[]
                            {
                                new FormQuestion
                                {
                                    Id = "comments",
                                    Label = "Comments",
                                    Type = QuestionType.TextArea,
                                    ValidationRules = new[]
                                    {
                                        new ValidationRule
                                        {
                                            Type = ValidationRuleType.MaxLength,
                                            Max = 250
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var context = new TagHelperContext(
                new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                "test");

            var output = new TagHelperOutput("fdcp-form-builder",
                new TagHelperAttributeList(),
                (result, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

            tagHelper.Process(context, output);

            var content = output.Content.GetContent();
            Assert.Contains("maxlength='250'", content);
            Assert.DoesNotContain("character-count", content);
        }

        [Fact]
        public void Process_WithFormValidationErrors_IncludesErrorSummaryForGCDSv039()
        {
            // Arrange
            var tagHelper = new FDCPFormBuilderTagHelper
            {
                Form = new FormDefinition
                {
                    Id = "testForm",
                    Title = "Test Form",
                    Action = "/submit",
                    Method = "post",
                    SubmitButtonText = "Submit",
                    Sections = new[]
                    {
                        new FormSection
                        {
                            Title = "Test Section",
                            Questions = new[]
                            {
                                new FormQuestion
                                {
                                    Id = "email",
                                    Label = "Email Address",
                                    Type = QuestionType.Email,
                                    IsRequired = true,
                                    ErrorMessage = "Please enter a valid email address",
                                    ValidationRules = new[]
                                    {
                                        new ValidationRule
                                        {
                                            Type = ValidationRuleType.Email
                                        }
                                    }
                                },
                                new FormQuestion
                                {
                                    Id = "phone",
                                    Label = "Phone Number",
                                    Type = QuestionType.Text,
                                    IsRequired = true,
                                    ErrorMessage = "Phone number is required"
                                }
                            }
                        }
                    }
                }
            };

            var context = new TagHelperContext(
                new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                "test");

            var output = new TagHelperOutput("fdcp-form-builder",
                new TagHelperAttributeList(),
                (result, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

            // Act
            tagHelper.Process(context, output);

            // Assert
            var content = output.Content.GetContent();
            
            // Verify form structure includes error summary capability for GCDS v0.39.0
            Assert.Contains("class='gc-form'", content);
            Assert.Contains("<gcds-error-summary", content);
            
            // Verify required GCDS validation attributes are present
            Assert.Contains("error-message=\"Please enter a valid email address\"", content);
            Assert.Contains("error-message=\"Phone number is required\"", content);
            
            // Verify input IDs are properly set for error linking
            Assert.Contains("input-id='email'", content);
            Assert.Contains("input-id='phone'", content);
            
            // Verify validation rules are applied
            Assert.Contains("type='email'", content);
            Assert.Contains("type='text'", content);
        }

        [Fact]
        public void Process_WithMultipleValidationRules_RendersAllGCDSValidationAttributes()
        {
            // Arrange
            var tagHelper = new FDCPFormBuilderTagHelper
            {
                Form = new FormDefinition
                {
                    Id = "testForm",
                    Title = "Test Form",
                    Action = "/submit",
                    Method = "post",
                    SubmitButtonText = "Submit",
                    Sections = new[]
                    {
                        new FormSection
                        {
                            Title = "Validation Test Section",
                            Questions = new[]
                            {
                                new FormQuestion
                                {
                                    Id = "username",
                                    Label = "Username",
                                    Type = QuestionType.Text,
                                    IsRequired = true,
                                    ErrorMessage = "Username must be between 3 and 20 characters",
                                    ValidateOnBlur = true,
                                    ValidationRules = new[]
                                    {
                                        new ValidationRule
                                        {
                                            Type = ValidationRuleType.MinLength,
                                            Min = 3
                                        },
                                        new ValidationRule
                                        {
                                            Type = ValidationRuleType.MaxLength,
                                            Max = 20
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var context = new TagHelperContext(
                new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                "test");

            var output = new TagHelperOutput("fdcp-form-builder",
                new TagHelperAttributeList(),
                (result, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

            // Act
            tagHelper.Process(context, output);

            // Assert
            var content = output.Content.GetContent();
            
            // Verify GCDS v0.39.0 validation attributes
            Assert.Contains("input-id='username'", content);
            Assert.Contains("name='username'", content);
            Assert.Contains("required", content);
            Assert.Contains("validate-on=\"blur\"", content);
            Assert.Contains("error-message=\"Username must be between 3 and 20 characters\"", content);
            
            // Verify length constraints are handled via validation rules, not HTML attributes
            // GCDS components use data-validation-rules instead of minlength/maxlength
            Assert.Contains("data-validation-rules", content);
        }

        [Fact]
        public void Process_WithRichText_RendersContainerWithoutDuplicateTextboxAria()
        {
            var tagHelper = new FDCPFormBuilderTagHelper
            {
                Form = new FormDefinition
                {
                    Id = "testForm",
                    Title = "Test Form",
                    Action = "/submit",
                    Method = "post",
                    SubmitButtonText = "Submit",
                    Sections = new[]
                    {
                        new FormSection
                        {
                            Title = "Project Details",
                            Questions = new[]
                            {
                                new FormQuestion
                                {
                                    Id = "projectSummary",
                                    Label = "Project summary",
                                    Hint = "Describe your project goals, milestones and outcomes.",
                                    Type = QuestionType.RichText,
                                    IsRequired = true,
                                    Placeholder = "Describe your project goals, milestones and outcomes.",
                                    Height = "260px"
                                }
                            }
                        }
                    }
                }
            };

            var context = new TagHelperContext(
                new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                "test");

            var output = new TagHelperOutput("fdcp-form-builder",
                new TagHelperAttributeList(),
                (result, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

            tagHelper.Process(context, output);

            var content = output.Content.GetContent();
            Assert.Contains("id='projectSummary_editor'", content);
            Assert.Contains("data-fdcp-rich-text='true'", content);
            Assert.Contains("data-for='projectSummary'", content);
            Assert.Contains("data-error-id='projectSummary_error'", content);
            Assert.Contains("<span class='fdcp-rich-text-label gcds-label' id='projectSummary_label'", content);
            Assert.Contains("id='projectSummary_hint'", content);
            Assert.Contains("aria-hidden='true'", content);
            Assert.DoesNotContain(" for='projectSummary'", content);
            Assert.DoesNotContain("<label class='fdcp-rich-text-label", content);
            Assert.DoesNotContain("role='textbox'", content);
            Assert.DoesNotContain("aria-multiline='true'", content);
            Assert.DoesNotContain("aria-labelledby='projectSummary_label'", content);
            Assert.DoesNotContain("aria-required='true'", content);
        }

        [Fact]
        public void Process_WithConditionalValidation_RendersCorrectDependencyAttributesForErrorSummary()
        {
            // Arrange
            var tagHelper = new FDCPFormBuilderTagHelper
            {
                Form = new FormDefinition
                {
                    Id = "testForm",
                    Title = "Test Form",
                    Action = "/submit",
                    Method = "post",
                    SubmitButtonText = "Submit",
                    Sections = new[]
                    {
                        new FormSection
                        {
                            Title = "Conditional Validation Section",
                            Questions = new[]
                            {
                                new FormQuestion
                                {
                                    Id = "hasAddress",
                                    Label = "Do you have an address?",
                                    Type = QuestionType.Radio,
                                    IsRequired = true,
                                    Options = new[]
                                    {
                                        new QuestionOption { Id = "yes", Value = "yes", Label = "Yes" },
                                        new QuestionOption { Id = "no", Value = "no", Label = "No" }
                                    }
                                },
                                new FormQuestion
                                {
                                    Id = "address",
                                    Label = "Address",
                                    Type = QuestionType.Text,
                                    IsRequired = false, // Conditionally required
                                    ErrorMessage = "Address is required when you have an address",
                                    Dependencies = new[]
                                    {
                                        new QuestionDependency
                                        {
                                            SourceQuestionId = "hasAddress",
                                            TargetQuestionId = "address",
                                            TriggerValue = "yes",
                                            Action = DependencyAction.Show
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var context = new TagHelperContext(
                new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                "test");

            var output = new TagHelperOutput("fdcp-form-builder",
                new TagHelperAttributeList(),
                (result, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

            // Act
            tagHelper.Process(context, output);

            // Assert
            var content = output.Content.GetContent();
            
            // Verify dependency attributes for proper error summary linking
            Assert.Contains("data-dependencies", content);
            
            // Verify radio group structure for GCDS v0.39.0 (uses gcds-radios component)
            Assert.Contains("name='hasAddress'", content);
            Assert.Contains("gcds-radios", content);
            Assert.Contains("legend='Do you have an address?'", content);
            
            // Verify text input has proper input-id
            Assert.Contains("input-id='address'", content);
        }
    }
}