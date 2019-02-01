// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;

namespace KFoxBot
{
    /// <summary>
    /// Represents a bot that processes incoming activities.
    /// For each user interaction, an instance of this class is created and the OnTurnAsync method is called.
    /// This is a Transient lifetime service.  Transient lifetime services are created
    /// each time they're requested. For each Activity received, a new instance of this
    /// class is created. Objects that are expensive to construct, or have a lifetime
    /// beyond the single turn, should be carefully managed.
    /// For example, the <see cref="MemoryStorage"/> object and associated
    /// <see cref="IStatePropertyAccessor{T}"/> object are created with a singleton lifetime.
    /// </summary>
    /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1"/>
    public class KFoxBot : IBot
    {
        private const string BotIntroduction = @"Hello User!!!!
                                                 My Name is KFox. I am your Shopping Assistant.";

        private readonly KFoxBotAccessors _accessors;

        private DialogSet _dialogs;

        public KFoxBot(KFoxBotAccessors accessors)
        {
            _accessors = accessors ?? throw new System.ArgumentNullException(nameof(accessors));
            _dialogs = new DialogSet(accessors.ConversationDialogState);

            // This array defines how the Waterfall will execute.
            var usernameSteps = new WaterfallStep[]
            {
                NameStepAsync,
                NameConfirmStepAsync,
                HelpConfirmStepAsync,
                ShopItemSuggestStepAsync,
                ProductChoiceStepAsync,
                FinalizeProductStepAsync,
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            _dialogs.Add(new WaterfallDialog("username", usernameSteps));
            _dialogs.Add(new TextPrompt("name"));
            _dialogs.Add(new ConfirmPrompt("confirm"));
            _dialogs.Add(new ChoicePrompt("choices"));
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Handle Message activity type, which is the main activity type for shown within a conversational interface
            // Message activities may contain text, speech, interactive cards, and binary or unknown attachments.
            // see https://aka.ms/about-bot-activity-message to learn more about the message and other activity types
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                if (turnContext == null)
                {
                    throw new System.ArgumentNullException(nameof(turnContext));
                }

                // Run the DialogSet - let the framework identify the current state of the dialog from
                // the dialog stack and figure out what (if any) is the active dialog.
                var dialogContext = await _dialogs.CreateContextAsync(turnContext, cancellationToken);
                var results = await dialogContext.ContinueDialogAsync(cancellationToken);

                // If the DialogTurnStatus is Empty we should start a new dialog.
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dialogContext.BeginDialogAsync("username", null, cancellationToken);
                }
            }

            // Handle Conversation Update activity type with an Intro of the Bot
            else if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate)
            {
                // Check if Null is for turnContext Members added
                if (turnContext.Activity.MembersAdded != null)
                {
                    await SendWelcomeMsgAsync(turnContext, cancellationToken);
                }
            }
            else
            {
                await turnContext.SendActivityAsync($"{turnContext.Activity.Type} event detected");
            }

            // Save the dialog state into the conversation state.
            await _accessors.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);

            // Save the user profile updates into the user state.
            await _accessors.UserState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        // This function will load options for User to choose the Shopping Items
        private static PromptOptions ShoppingItems(Activity activity, string userName)
        {
            var options = new PromptOptions()
            {
                Prompt = activity.CreateReply($"So {userName}, What would you like to buy?"),
                Choices = new List<Choice>(),
            };

            options.Choices.Add(new Choice() { Value = "Clothes" });
            options.Choices.Add(new Choice() { Value = "Watches" });
            options.Choices.Add(new Choice() { Value = "Glasses" });
            options.Choices.Add(new Choice() { Value = "Footwear" });

            return options;
        }

        // This function will load options for User to choose the type of Clothes
        private static PromptOptions ClothesItems(Activity activity, string userName)
        {
            var options = new PromptOptions()
            {
                Prompt = activity.CreateReply($"So {userName}, Which type of Clothes would you like to buy?"),
                Choices = new List<Choice>(),
            };

            options.Choices.Add(new Choice() { Value = "Shirts" });
            options.Choices.Add(new Choice() { Value = "Trousers" });
            options.Choices.Add(new Choice() { Value = "T-Shirts" });
            options.Choices.Add(new Choice() { Value = "Shorts" });

            return options;
        }

        // This function will load options for User to choose the type of Watches
        private static PromptOptions WatchItems(Activity activity, string userName)
        {
            var options = new PromptOptions()
            {
                Prompt = activity.CreateReply($"So {userName}, Which type of Watch would you like to buy?"),
                Choices = new List<Choice>(),
            };

            options.Choices.Add(new Choice() { Value = "Formal" });
            options.Choices.Add(new Choice() { Value = "Sports" });
            options.Choices.Add(new Choice() { Value = "Metal Body" });
            options.Choices.Add(new Choice() { Value = "Smart" });

            return options;
        }

        // This function will load options for User to choose the type of Glasses
        private static PromptOptions GlassesItems(Activity activity, string userName)
        {
            var options = new PromptOptions()
            {
                Prompt = activity.CreateReply($"So {userName}, Which type of Glasses would you like to buy?"),
                Choices = new List<Choice>(),
            };

            options.Choices.Add(new Choice() { Value = "Sun Glasses" });
            options.Choices.Add(new Choice() { Value = "Frameless" });

            return options;
        }

        // This function will load options for User to choose the type to Footwear
        private static PromptOptions FootwearItems(Activity activity, string userName)
        {
            var options = new PromptOptions()
            {
                Prompt = activity.CreateReply($"So {userName}, Which type of Footwear would you like to buy?"),
                Choices = new List<Choice>(),
                RetryPrompt = activity.CreateReply($"Please select from above options"),
            };

            options.Choices.Add(new Choice() { Value = "Shoes" });
            options.Choices.Add(new Choice() { Value = "Loafers" });
            options.Choices.Add(new Choice() { Value = "Sandals" });
            options.Choices.Add(new Choice() { Value = "Floaters" });

            return options;
        }

        // This fucntion will load options for User to choose Mall to shop the item opted
        private static PromptOptions CompanyNames(Activity activity, string userName)
        {
            var options = new PromptOptions()
            {
                Prompt = activity.CreateReply($"So {userName}, From which Shop would you like to buy?"),
                Choices = new List<Choice>(),
            };

            options.Choices.Add(new Choice() { Value = "Central" });
            options.Choices.Add(new Choice() { Value = "Shoppers Stop" });
            options.Choices.Add(new Choice() { Value = "UB City Mall" });
            options.Choices.Add(new Choice() { Value = "Meenakshi Mall" });

            return options;
        }

        // Function for creating Welcome message to the User
        private static async Task SendWelcomeMsgAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in turnContext.Activity.MembersAdded)
            {
                // In turn Context members, turnContext.Activty.Recipient.Id is the Bot
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    var welcomeReply = turnContext.Activity.CreateReply();
                    welcomeReply.Text = BotIntroduction;
                    await turnContext.SendActivityAsync(welcomeReply, cancellationToken);
                }
            }
        }

        // Waterfall step function for asking the User his name
        private static async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            // Running a prompt here means the next WaterfallStep will be run when the users response is received.
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("May I know your name please??"), cancellationToken);
            return await stepContext.PromptAsync("name", new PromptOptions { Prompt = MessageFactory.Text("<< In the format 'It is NAME' >>") }, cancellationToken);
        }

        // Waterfall step function for saving his name and asking the User, if he needs the Bot's help
        private async Task<DialogTurnResult> NameConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Get the current profile object from user state.
            var userProfile = await _accessors.UserProfile.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            var nameSentence = (string)stepContext.Result;
            var nameSentenceArray = nameSentence.Split(' ');
            var userProfileName = string.Empty;

            // Loop for separating the User's name from the sentence
            if (nameSentenceArray.Length > 3)
            {
                if (nameSentenceArray[0].ToLowerInvariant() == "it" && nameSentenceArray[1].ToLowerInvariant() == "is")
                {
                    for (int i = 2; i < nameSentenceArray.Length; i++)
                    {
                        userProfileName = userProfileName + nameSentenceArray[i] + " ";
                    }

                    userProfileName = userProfileName.TrimEnd();
                }
                else
                {
                    // Greet user and Close the dialog activity
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Sorry!!! I am unable to understand you."), cancellationToken);

                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                }
            }
            else if (nameSentenceArray.Length == 3)
            {
                userProfileName = nameSentenceArray[2];
            }
            else
            {
                // Greet user and Close the dialog activity
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Sorry!!! I am unable to understand you."), cancellationToken);

                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }

            // Update the profile.
            userProfile.Name = userProfileName;

            // Prompt Yes or No for User to continue the Execution
            return await stepContext.PromptAsync("confirm", new PromptOptions { Prompt = MessageFactory.Text($"Okay {userProfile.Name}. Would you like to have my assistance in your Shopping??") }, cancellationToken);
        }

        // Waterfall step function for expressing Bot's response and displaying choices for Shopping Items
        private async Task<DialogTurnResult> HelpConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Get the current profile object from user state.
            var userProfile = await _accessors.UserProfile.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            // If User wants Bot's assistance
            if ((bool)stepContext.Result)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("I am very happy to help you!!"), cancellationToken);

                // Prompt pre-deifined choices for User select his choice 
                return await stepContext.PromptAsync("choices", ShoppingItems(stepContext.Context.Activity, (string)userProfile.Name), cancellationToken);
            }
            else
            {
                // Greet user and Close the dialog activity
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Okay!! Pleasure talking to you {userProfile.Name}!!!"), cancellationToken);

                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
        }

        // Waterfall step function for displaying options for each type of Shopping Item
        private async Task<DialogTurnResult> ShopItemSuggestStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Get the current profile object from user state
            var userProfile = await _accessors.UserProfile.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            // Save the Shopping Item choosen by the User
            userProfile.ShoppingItem = (string)stepContext.Context.Activity.Text;

            // Loop for validating the response from User
            if (userProfile.ShoppingItem.ToLowerInvariant() == "clothes")
            {
                // Prompt pre-defined choices for User select his choice
                return await stepContext.PromptAsync("choices", ClothesItems(stepContext.Context.Activity, (string)userProfile.Name), cancellationToken);
            }
            else if (userProfile.ShoppingItem.ToLowerInvariant() == "watches")
            {
                // Prompt pre-defined choices for User select his choice
                return await stepContext.PromptAsync("choices", WatchItems(stepContext.Context.Activity, (string)userProfile.Name), cancellationToken);
            }
            else if (userProfile.ShoppingItem.ToLowerInvariant() == "glasses")
            {
                // Prompt pre-defined choices for User select his choice
                return await stepContext.PromptAsync("choices", GlassesItems(stepContext.Context.Activity, (string)userProfile.Name), cancellationToken);
            }
            else
            {
                // Prompt pre-defined choices for User select his choice
                return await stepContext.PromptAsync("choices", FootwearItems(stepContext.Context.Activity, (string)userProfile.Name), cancellationToken);
            }
        }

        // Waterfall step function for validating the selected item
        private async Task<DialogTurnResult> ProductChoiceStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Get the current profile object from user state.
            var userProfile = await _accessors.UserProfile.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            // Save Shopping Item choosen by the User
            userProfile.ShoppingProduct = (string)stepContext.Context.Activity.Text;

            return await stepContext.PromptAsync("choices", CompanyNames(stepContext.Context.Activity, (string)userProfile.Name), cancellationToken);
        }

        // Waterfall step function for summarizing the User's choice
        private async Task<DialogTurnResult> FinalizeProductStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Get the current profile object from user state.
            var userProfile = await _accessors.UserProfile.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            // Save Shopping Mall choosen by the User
            userProfile.ShoppingMall = (string)stepContext.Context.Activity.Text;

            // Send the summary of the Conversation
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Okay {userProfile.Name}"), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"As decided, we will go to {userProfile.ShoppingMall} and buy {userProfile.ShoppingProduct} for you!!"), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"It was fun assisting you in your Shopping!!!"), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Bye {userProfile.Name}!!!"), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Nice to meet you!!!"), cancellationToken);

            // Close the dialog activity
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}