using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MemoryScript", menuName = "ScriptableObjects/MemoryScript", order = 1)]
public class ScriptableMemoryScript : ScriptableObject
{
    public List<memoryLine> lines = new();
    public List<memorySpeaker> speakers = new();

    ScriptableMemoryScript()
    {
        lines.Add(new memoryLine());
        speakers.Add(new memorySpeaker());
    }
}

[System.Serializable]
public class memoryLine
{
    public int speaker = 0;
    [Multiline]
    public string line = "placeholder";
}

[System.Serializable]
public class memorySpeaker
{
    public string name = "placeholder";
    public Color color = Color.white;
    public memorySpeaker()
    {
        color.a = 1f;
    }
}

//old memory texts:
/*
readonly public string[][] memory_texts = { 
        new string[]{ 
            "2&What's your earliest childhood memory?", 
            "1&Don't have any.",
            "2&What do you mean?", 
            "1&My first memory is from two weeks after my fifthteenth birthday.", 
            "2&Nothing before that?", 
            "1&Nope. Why?",
            "2&Bestie,.. that's not normal.", 
            "1&What do you mean?"
            }, //Memoryless
        new string[]{ 
            "1&Well, here I am.", 
            "1&Home sweet childhood home."
            }, //Childhood Home
        new string[]{
            "R&Oh what a handsome young man. You've gotten so big. Here, I brought presents.", 
            "1&Oh, uh... Thanks Aunt Rosemary.",
            "0& ",
            "M&You could of at least pretended to be excited for Aunt Rosemary's gift. She think really hard about what clothes you'd like.",
            "M&Even if she got it wrong this year there's no reason to sound so ungrateful.", 
            "1&She gets it wrong every year." 
            }, //Aunt Rosemary's Gift
        new string[]{ 
            }, //Christmas reprisal
        new string[]{ 
            "D&Stop your baby-crying boy. It's just a scratch. Real men don't cry about small shit like this.", 
            "1&*sniff*"
            }, //Stop crying
        new string[]{ 
            "1&I want the pink one.", 
            "M&Now Son, you know Jessica wants the pink one. Why not let her have it.", 
            "1&She always gets to have the pink one.", 
            "M&Of course she does; she's a girl, Son."
            }, //The Pink One
        new string[]{ 
            "D&No Son of mine is going to play with Dolls.", 
            "1&Dad stop it, please stop.", 
            "D&Quit crying boy, before I give you something real to cry about."
            }, //Dolls
        new string[]{ 
            "1&Hey guys.",
            "3&Woah dude! You scared the shit out of me. How did you learn to move so silently.", 
            "1&My mom likes her quiet time and our floorboards creak."
            }, //Sneaky
        new string[]{ 
            "J&Bro, wake up! Naptime's over.",
            "1&Wish I could just nap forever.",
            "J&What?",
            "1&You know, lay down and never wake up.", 
            "J&Bro, you alright?", 
            "1&Yeah? ... Forget about it, alright."
            }, //Naptime
        new string[]{ 
            "1&Hey Jess, I was thinking; if you wanted to, you could call me Lily.", 
            "J&What? But isn't that a girl's name: wouldn't it be weird.", 
            "1&... I said if you wanted to.",
            "1b&I guess Lily was a stupid nickname anyway."
            }, //Lily
        new string[]{ 
            "D&<b>That's it!</b> This is ridiculous. I'm taking you to the barber and you're getting a haircut.", 
            "1&But Dad, I like my hair long.", 
            "D&And if you give me anymore lip, I'll have George shave you." 
            }, //Haircuts
        new string[]{ 
            "D&Why don't you talk to me anymore?", 
            "1&..."
            }, //Talk to me
        new string[]{ 
            "D&I lost my son, and you're saying I'm not even allowed to grieve.", 
            "1&You didn't lose anything. I'm still here, same as I ever was.", 
            "1&I'm just not pretending to be the man you think I was supposed to be."
            }, //Grieving
        new string[]{ 
            "D&We talked with Aunt Rosemary about your situation.",
            "M&She recommended we enroll you in a summer camp of sorts.", 
            "1&I thought I told old you not to tell her."
            }, // 'Summer Camp'
        new string[]{ 
            "D&Young man, you are the child and we're the adults. When we listen to you, it is as a courtesy.", 
            "D&It's not something you can demand, especially against your best interest.", 
            "D&Now got pack your fucking bags."
            }, //Courtesy
        new string[]{ 
            "Doll&Oh deary you, what a wretched life.", 
            "1&Yeah it kinda sucked.", 
            "Doll&Would you like to forget about it?", 
            "Doll&I can help with that."
            }, //Wretch Lift
        new string[]{ 
            "1&Why?", 
            "Doll&Because I'm hungry, and your look frayed enough to agree to it.", 
            "1&...Yeah okay then. Do it.", 
            "Doll&Don't worry sweetie, I'll be thorough."
            }, //Forget about all that 
    };
    
    
    
    readonly public string[][] special_memory_texts = {
        new string[]{ 
            "Don't forget your umbrella. It's supposed to rain all weekend.", 
            "My umbrella?", 
            "My umbrella...", 
            "MY UMBRELLA!"
        },
        new string[]{ 
            "You never get used to the sensation of falling.", 
            "It feels so freeing, like you've escaped gravity's cruel prison.", 
            "But then, the Ground is a harsh Warden, and she always catches her runaways."
        },
        new string[]{ "Lily lily lily, like the flower."}
        };

        
    public string[][] mr_418_exception_speech= new string[3][]{ new string[3]{ "Mr. 418", "I don't know how ya managed it kiddo, but cha tried to remember something that happened before yous were born.", "418"}, 
		                                                        new string[2]{ "Mr. 418", "Sorry, only way I know to fix the timeline is to start cha over. Whelp, should be fixed now; give it another shot."   },
														        new string[2]{ "Mr. 418", "Be warned though, I can't gaurantee there are enough whips about to unlock the Boss door. You might be better off with a full reset."} };
	public string[][] ms_429_exception_speech= new string[2][]{ new string[3]{ "Ms. 429", "Oh bless your heart darlin'. Your love the game so much, you found an extra wisp of memory, but we ain't got no more story left for ya.", "429"}, 
		                                                        new string[2]{ "Ms. 429", "Why don't you head on over to the final boss arena and see how this all ends?" } };
	public string mx_404_exception_name= "Mx 404, Myst. Stranger";

    */