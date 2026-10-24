namespace BinSite.Social.Services;
/// <summary>Original editorial prompts; selecting one fills a draft and never fabricates a public user post.</summary>
public static class EditorialPrompts
{
 public static readonly StoryPrompt[] Stories=[
  new("The Letter on the Last Ferry","After the final passenger left, an envelope remained on a seat. It bore the captain's name and listed every stop he would pass tomorrow. At the bottom was an island that did not exist in the sea."),
  new("The Missing Monday","Everyone in town woke on Tuesday. Phones, calendars and newspapers agreed that Monday had never happened. Only the baker was holding the bread he had baked on Monday, still warm."),
  new("The Neighbor's Future","Every night at three, an unwritten song came from next door. When it finally played on the radio, the presenter named its composer: a baby born yesterday. She raised her hand to knock."),
  new("The Sky Library","Bookmarks fell instead of rain. Each carried the last sentence a different person had read. When the child found his own bookmark, he discovered that its sentence had not yet begun."),
  new("Secondhand Shadows","The shop opened only at sunset. Shadows separated from their owners waited in the window under little labels. The most expensive resembled nobody. The shopkeeper said it belonged to tomorrow."),
  new("The Silent Station","The station announcements had been silent for a week. Then one platform whispered: 'Board this train and you will find something you lost, but you cannot choose what it is.'"),
  new("The Tailor's Hidden Pockets","The tailor sewed an invisible pocket into every jacket. Customers reached inside and touched forgotten memories instead of objects. One day a woman arrived whose pockets had all become empty."),
  new("The Sea's Invoice","The fisherman pulled a sealed invoice from his net. It itemized everything the sea had taken. At the bottom, someone requested a signature to return a ship nobody remembered."),
  new("The Last Adult Who Could Play","The children woke to find that all the adults had forgotten how to play. Only the old park keeper remembered the rules. He called them over and drew a hopscotch grid nobody recognized."),
  new("One Extra Room","The house had always held two bedrooms and a living room. During a power cut, a third door appeared at the end of the hall. Light beneath it showed the house thirty years from now."),
  new("The Repairer of Lost Sounds","Jars held the creak of a door, applause from a demolished cinema and an apology never spoken. The apprentice opened the jar bearing his own name."),
  new("The Moon's Return Day","The moon looked unusually close. A one-page notice told the city that wishes hung in the sky would be returned to their owners. The first parcel landed on an apartment roof."),
  new("The Minute Garden","The gardener grew minutes instead of flowers. Some passed as quickly as laughter; others weighed as heavily as farewells. When all the clocks stopped, a single bud remained closed."),
  new("The Invisible Orchestra","The concert hall had no musicians, but every seat was taken. When the conductor lifted her hand, everyone heard an instrument from childhood. A woman in the front row heard a sound from tomorrow."),
  new("The Back of the Return Ticket","The ticket's front named his home address. Its back held the handwriting of someone he had not met: 'Do not knock three times when you return.' The driver saw it and offered him his own seat.")
 ];
 public static readonly PollPrompt[] Polls=[
  new("The neighborhood can open one place. Which should it be?","A late-night board-game library","A tiny studio where anyone can make a short film"),
  new("What should be the main event of a city festival?","A shared story game unfolding across streets","A silent concert with visitor-designed stages"),
  new("What would you want most from a new social network?","Small groups that make things together","Daily creative challenges between friends"),
  new("Which weekend game would you play with friends?","A city mystery solved by collecting clues","An improv contest where household objects speak"),
  new("If a game had only one rule, which would you choose?","Change the previous rule after every move","You cannot score without helping an opponent"),
  new("What should be the heart of a community app?","An hour-for-an-hour skill exchange","Shared projects completed by strangers"),
  new("Which museum experience would you choose?","Visitors complete unfinished inventions","Explore an object's past through sound"),
  new("Which entertainment channel would bring you back?","Contests where viewers change the rules","A series with a different ending writer each episode"),
  new("What should make a new word game different?","Draw a word's sound instead of defining it","Earn letters inside a friend's puzzle"),
  new("Which surprise would you prepare for a celebration?","A digital treasure hunt with a clue from every friend","A shared time capsule opening in a year"),
  new("Which shared-workspace idea is more appealing?","Sound rooms organized around tasks","Teams exchanging ten minutes of project help"),
  new("How should a story app involve its readers?","A new writer takes over each paragraph","Readers choose what the character remembers"),
  new("Which playful cafe idea would you try?","A weekly drink recipe chosen by customers","A small table mystery attached to a drink"),
  new("Which contest achievement should matter more?","Help everyone win in the fewest moves","Teach your own solution to someone else"),
  new("What should a digital collection help you create?","A personal map made from places' sounds","An atlas linking things you have learned")
 ];
}
