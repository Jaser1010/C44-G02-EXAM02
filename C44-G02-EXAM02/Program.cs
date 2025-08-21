#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace C44_G02_EXAM02
{
    #region Answer Class
    public class Answer
    {
        public int AnswerId { get; set; }
        public string AnswerText { get; set; }

        public Answer(int answerId, string answerText)
        {
            AnswerId = answerId;
            AnswerText = answerText;
        }

        // Overriding ToString for easy display.
        public override string ToString()
        {
            return $"{AnswerId}. {AnswerText}";
        }
    }
    #endregion

    #region Questions Classes

    #region Question Class
    public abstract class Question
    {
        public string Header { get; set; }
        public string Body { get; set; }
        public int Mark { get; set; }
        public Answer[] AnswerList { get; set; } 
        public Answer RightAnswer { get; set; }

        public Question(string header, string body, int mark)
        {
            Header = header;
            Body = body;
            Mark = mark;
            AnswerList = Array.Empty<Answer>();
            RightAnswer = new Answer(0, ""); // Default initialization to satisfy compiler
        }

        public abstract void Display();

        // Overriding ToString for a standard question format. 
        public override string ToString()
        {
            return $"{Header}\n{Body} ({Mark} Marks)";
        }
    }
    #endregion

    #region MCQ Question Class
    public class MCQQuestion : Question
    {
        // Constructor chaining from base Question class.
        public MCQQuestion(string body, int mark, Answer[] answerList, Answer rightAnswer): base("Multiple Choice Question", body, mark)
        {
            AnswerList = answerList;
            RightAnswer = rightAnswer;
        }

        public override void Display()
        {
            Console.WriteLine(this); // Calls base ToString()
            foreach (var answer in AnswerList)
            {
                Console.WriteLine(answer);
            }
        }
    }
    #endregion

    #region True Or False Question Class
    public class TrueOrFalseQuestion : Question
    {
        // Constructor chaining. [cite: 30]
        public TrueOrFalseQuestion(string body, int mark, Answer rightAnswer): base("True or False Question", body, mark)
        {
            AnswerList = new Answer[]
            {
            new Answer(1, "True"),
            new Answer(2, "False")
            };
            /// Using First() ensures a valid answer is found or throws an exception, preventing nulls.
            RightAnswer = AnswerList.First(a => a.AnswerId == rightAnswer.AnswerId);
        }

        public override void Display()
        {
            Console.WriteLine(this); // Calls base ToString()
            foreach (var answer in AnswerList)
            {
                Console.WriteLine(answer);
            }
        }
    }
    #endregion

    #endregion

    #region Exams Classes

    #region Exam Class
    public abstract class Exam : IComparable<Exam>
    {
        public TimeSpan TimeOfExam { get; set; }
        public int NumberOfQuestions { get; set; }
        public List<Question> Questions { get; set; }

        public Exam(TimeSpan time, int numberOfQuestions)
        {
            TimeOfExam = time;
            NumberOfQuestions = numberOfQuestions;
            Questions = new List<Question>();
        }

        public abstract void ShowExam();

        public int CompareTo(Exam? other)
        {
            if (other is null) return 1;
            return this.NumberOfQuestions.CompareTo(other.NumberOfQuestions);
        }

        public override string ToString()
        {
            return $"Exam Details: {NumberOfQuestions} questions, Time Limit: {TimeOfExam.TotalMinutes} minutes.";
        }
    }
    #endregion

    #region Final Exam Class
    public class FinalExam : Exam
    {
        public FinalExam(TimeSpan time, int numberOfQuestions) : base(time, numberOfQuestions) { }

        public override void ShowExam()
        {
            Console.WriteLine("--- Starting Final Exam ---");
            int totalGrade = 0;
            int userScore = 0;
            var userAnswers = new Dictionary<Question, Answer>();

            foreach (var question in Questions)
            {
                totalGrade += question.Mark;
                question.Display();
                Console.Write("Your answer (enter ID): ");
                int userAnswerId = Program.GetValidIntInput(1, question.AnswerList.Length);

                var chosenAnswer = question.AnswerList.First(a => a.AnswerId == userAnswerId);
                userAnswers[question] = chosenAnswer;

                if (chosenAnswer.AnswerId == question.RightAnswer.AnswerId)
                {
                    userScore += question.Mark;
                }
                Console.WriteLine("--------------------------");
            }

            Console.WriteLine("\n--- Final Exam Results ---");
            Console.WriteLine($"Your Grade: {userScore} out of {totalGrade}");
            Console.WriteLine("\nReview your answers:");
            foreach (var entry in userAnswers)
            {
                Console.WriteLine($"\nQ: {entry.Key.Body}");
                Console.WriteLine($"Your Answer: {entry.Value.AnswerText}");
                Console.WriteLine($"Correct Answer: {entry.Key.RightAnswer.AnswerText}");
            }
        }
    }
    #endregion

    #region Practical Exam Class
    public class PracticalExam : Exam
    {
        public PracticalExam(TimeSpan time, int numberOfQuestions) : base(time, numberOfQuestions) { }

        public override void ShowExam()
        {
            Console.WriteLine("--- Starting Practical Exam ---");

            foreach (var question in Questions)
            {
                question.Display();
                Console.Write("Your answer (enter ID): ");
                _ = Program.GetValidIntInput(1, question.AnswerList.Length); // Answer is taken but not scored
                Console.WriteLine("--------------------------");
            }

            Console.WriteLine("\n--- Practical Exam Review ---");
            Console.WriteLine("The correct answers are shown below:");
            foreach (var question in Questions)
            {
                Console.WriteLine($"\nQ: {question.Body}");
                Console.WriteLine($"Correct Answer: {question.RightAnswer}");
            }
        }
    }
    #endregion

    #endregion

    #region Subject Class
    public class Subject : ICloneable
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }
        // The Exam is nullable because a Subject can exist before its exam is created.
        public Exam? SubjectExam { get; set; }

        public Subject(int id, string name)
        {
            SubjectId = id;
            SubjectName = name;
        }

        public void CreateExam()
        {
            Console.WriteLine($"\n--- Creating Exam for {SubjectName} ---");
            Console.WriteLine("Choose Exam Type:\n1. Final Exam\n2. Practical Exam");
            int examType = Program.GetValidIntInput(1, 2);

            Console.Write("Enter the time for the exam (in minutes): ");
            int timeInMinutes = Program.GetValidIntInput(1, 180);

            Console.Write("Enter the number of questions for the exam: ");
            int numQuestions = Program.GetValidIntInput(1, 50);

            SubjectExam = examType == 1
                ? new FinalExam(TimeSpan.FromMinutes(timeInMinutes), numQuestions)
                : new PracticalExam(TimeSpan.FromMinutes(timeInMinutes), numQuestions);

            for (int i = 0; i < numQuestions; i++)
            {
                Console.WriteLine($"\n--- Adding Question {i + 1}/{numQuestions} ---");
                int questionType = 2; // Default to MCQ
                if (examType == 1) // Final exam can have T/F or MCQ
                {
                    Console.WriteLine("Choose Question Type:\n1. True or False\n2. MCQ (Multiple Choice)");
                    questionType = Program.GetValidIntInput(1, 2);
                }
                else
                {
                    Console.WriteLine("Practical exams only support MCQ questions.");
                }

                Console.Write("Enter the body of the question: ");
                string body = Program.GetValidStringInput();

                Console.Write("Enter the mark for this question: ");
                int mark = Program.GetValidIntInput(1, 100);

                if (questionType == 1) // True or False
                {
                    Console.Write("What is the correct answer? (1 for True, 2 for False): ");
                    int rightAnswerId = Program.GetValidIntInput(1, 2);
                    var rightAnswer = new Answer(rightAnswerId, rightAnswerId == 1 ? "True" : "False");
                    SubjectExam.Questions.Add(new TrueOrFalseQuestion(body, mark, rightAnswer));
                }
                else // MCQ
                {
                    Console.Write("How many answer choices for this question? ");
                    int numChoices = Program.GetValidIntInput(2, 10);
                    var answers = new Answer[numChoices];
                    for (int j = 0; j < numChoices; j++)
                    {
                        Console.Write($"Enter text for answer choice {j + 1}: ");
                        string answerText = Program.GetValidStringInput();
                        answers[j] = new Answer(j + 1, answerText);
                    }

                    Console.WriteLine("Current Choices:");
                    foreach (var ans in answers) Console.WriteLine(ans);

                    Console.Write("Which choice is the correct answer? (Enter ID): ");
                    int rightAnswerId = Program.GetValidIntInput(1, numChoices);
                    var rightAnswer = answers.First(a => a.AnswerId == rightAnswerId);
                    SubjectExam.Questions.Add(new MCQQuestion(body, mark, answers, rightAnswer));
                }
            }
            Console.WriteLine("\n✅ Exam created successfully!");
        }

        public object Clone()
        {
            return new Subject(this.SubjectId, this.SubjectName)
            {
                SubjectExam = this.SubjectExam // Note: Shallow copy of the exam
            };
        }

        public override string ToString()
        {
            return $"Subject: {SubjectName} (ID: {SubjectId})";
        }
    }
    #endregion



    internal class Program
    {
        #region Helper function for validated integer input.
        public static int GetValidIntInput(int min, int max)
        {
            int value;
            // Check for null from Console.ReadLine()
            while (!int.TryParse(Console.ReadLine(), out value) || value < min || value > max)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"Invalid input. Please enter a number between {min} and {max}: ");
                Console.ResetColor();
            }
            return value;
        }
        #endregion

        #region Helper function for validated non-empty string input.
        public static string GetValidStringInput()
        {
            string? input = Console.ReadLine();
            while (string.IsNullOrWhiteSpace(input))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Input cannot be empty. Please try again: ");
                Console.ResetColor();
                input = Console.ReadLine();
            }
            return input;
        }
        #endregion
        static void Main(string[] args)
        {


            Console.WriteLine("Welcome to the Exam Creator!");
            Console.WriteLine("--- Subject Setup ---");
            Console.Write("Please enter the Subject ID: ");
            int subjectId = GetValidIntInput(1, int.MaxValue);

            Console.Write("Please enter the Subject Name: ");
            string subjectName = GetValidStringInput();

            var userSubject = new Subject(subjectId, subjectName);

            userSubject.CreateExam();

            Console.Write("\nDo you want to start the exam now? (yes/no): ");
            // Handle nullable return from ReadLine()
            string? startChoice = Console.ReadLine()?.ToLower();

            if (startChoice == "yes")
            {
                Console.WriteLine("\nPress any key to start the exam...");
                Console.ReadKey();
                Console.Clear();
                // Use the null-conditional operator ?. to safely call ShowExam()
                userSubject.SubjectExam?.ShowExam();
            }
            else
            {
                Console.WriteLine("Exam is ready. You can take it later. Goodbye!");
            }

        }
    }
}

