using System;
using System.Collections.Generic;

namespace CS_Grinder
{

	public enum CardTypesEnum { Artifact, Creature, Enchantment, Instant, Land, Sorcery }

	public enum ComboPiecesEnum { InfiniteMana, InfiniteCreatureUntaps, InfiniteLandUntaps, CreaturesAreLands, InfiniteDamage }

	class Card
	{
		public string name { get; set; }
		public string role { get; set; }
		public CardTypesEnum type { get; set; }

		public List<Ability> Abilities { get; set; }

		public Card()
		{
			this.name = "";
			this.role = "";
			this.type = CardTypesEnum.Artifact;
			this.Abilities = new List<Ability>();
		}

		public Card(string name, string role, CardTypesEnum type)
		{
			this.name = name;
			this.role = role;
			this.type = type;
			this.Abilities = new List<Ability>();
		}

		public string Print(bool print = true)
		{
			string printString = $"{this.name} | {this.type}";
			if (print)
			{
				System.Console.WriteLine(printString);
			}
			return printString;
		}

		public Card Copy()
		{
			Card copiedCard = new Card(this.name, this.role, this.type);
			foreach (Ability ability in this.Abilities)
			{
				copiedCard.Abilities.Add(ability.Copy());
			}
			return copiedCard;
		}
	}

	class Ability
	{
		public string name { get; set; }
		public HashSet<ComboPiecesEnum> requirements { get; set; }
		public HashSet<ComboPiecesEnum> results { get; set; }
		public bool isEnabled { get; set; }
		public bool isPayoff { get; set; }

		public Ability(string name = "", bool isPayoff = false, bool isEnabled = false)
		{
			this.name = name;
			this.isPayoff = isPayoff;
			this.isEnabled = isEnabled;
			this.requirements = new HashSet<ComboPiecesEnum>();
			this.results = new HashSet<ComboPiecesEnum>();
		}

		public Ability Copy()
		{
			Ability copy = new Ability(name, isPayoff);
			foreach (ComboPiecesEnum requirement in this.requirements)
			{
				copy.requirements.Add(requirement);
			}
			foreach (ComboPiecesEnum result in this.results)
			{
				copy.results.Add(result);
			}
			return copy;
		}
	}

	abstract class CardCollection
	{
		public string name { get; set; }
		public List<Card> AllCards { get; set; }

		public bool AddCard(Card card)
		{
			this.AllCards.Add(card);
			return true;
		}

		public Card drawCard()
		{
			Card card = AllCards[0];
			AllCards.RemoveAt(0);
			return card;
		}

		public List<Card> Shuffle()
		{
			List<Card> newCards = new List<Card>();
			Random r = new Random();
			int i;

			while (AllCards.Count > 0)
			{
				i = r.Next(AllCards.Count);
				newCards.Add(AllCards[i]);
				AllCards.RemoveAt(i);
			}

			AllCards = newCards;
			return newCards;
		}
	}

	class CardLibrary : CardCollection
	{
		public Card commander { get; set; }

		public CardLibrary()
		{
			this.name = "";
			this.AllCards = new List<Card>();
			this.commander = new Card();
		}

		public CardLibrary(string name)
		{
			this.name = name;
			this.AllCards = new List<Card>();
			this.commander = new Card();
		}

		public bool returnCards(Hand hand)
		{
			foreach (Card card in hand.AllCards)
			{
				this.AllCards.Add(card);
				hand.AllCards.Remove(card);
			}
			return true;
		}

		public string Print(bool print = true)
		{
			string printString = $"Library {this.name} - Commander: {this.commander.name} - Cards: {this.AllCards.Count}";
			foreach (Card LibraryCard in this.AllCards)
			{
				// LibraryCard.Print();
				printString += "\n" + LibraryCard.Print(false);
			}
			if (print)
			{
				System.Console.WriteLine(printString);
			}
			return printString;
		}
	}

	class Hand : CardCollection
	{
		public List<Ability> AllAbilities { get; set; }
		public HashSet<ComboPiecesEnum> ActivePieces { get; set; }
		public bool hasPayoff { get; set; }

		public Hand()
		{
			AllAbilities = new List<Ability>();
			ActivePieces = new HashSet<ComboPiecesEnum>();
			hasPayoff = false;
			this.name = "";
			this.AllCards = new List<Card>();
		}

		public void ExamineCard(Card newCard)
		{
			System.Console.WriteLine($"Examining card: {newCard.name}");
			this.AllCards.Add(newCard);
			bool doCheckAbilities = false;
			foreach (Ability cardAbility in newCard.Abilities)
			{
				if (!this.AllAbilities.Contains(cardAbility))
				{
					doCheckAbilities = true;
					AllAbilities.Add(cardAbility);
					if (cardAbility.isEnabled)
					{
						activateAbility(cardAbility);
					}
				}
			}
			while (doCheckAbilities)
			{
				doCheckAbilities = false;
				foreach (Ability abilty in this.AllAbilities)
				{
					if (!abilty.isEnabled)
					{
						int satisfiedRequirements = 0;
						foreach (ComboPiecesEnum requirement in abilty.requirements)
						{
							if (this.ActivePieces.Contains(requirement))
							{
								satisfiedRequirements += 1;
							}
						}
						if (satisfiedRequirements >= abilty.requirements.Count)
						{
							System.Console.WriteLine(abilty.name + " activated!");
							activateAbility(abilty);
							doCheckAbilities = true;
						}
					}
				}
			}
		}

		void activateAbility(Ability ability)
		{
			ability.isEnabled = true;
			foreach (ComboPiecesEnum piece in ability.results)
			{
				this.ActivePieces.Add(piece);
			}
			if (ability.isPayoff)
			{
				this.hasPayoff = true;
				System.Console.WriteLine($"Success! Payoff ability {ability.name} has been activated!");
			}
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			Card ashaya = new Card("Ashaya", "Commander", CardTypesEnum.Creature);
			Card forest = new Card("Forest", "Land", CardTypesEnum.Land);
			Card leyWeaver = new Card("Ley Weaver", "Untap", CardTypesEnum.Creature);
			Card nylea = new Card("Nylea, Keen-Eyed", "Payoff", CardTypesEnum.Creature);

			Ability ashayaAb = new Ability("Ashaya Ability", false, true);
			ashayaAb.results.Add(ComboPiecesEnum.CreaturesAreLands);
			Ability forestAb = new Ability("Forest Ability", false);
			forestAb.requirements.Add(ComboPiecesEnum.InfiniteLandUntaps);
			forestAb.results.Add(ComboPiecesEnum.InfiniteMana);
			Ability leyWeaverAb = new Ability("Ley Weaver Ability", false);
			leyWeaverAb.requirements.Add(ComboPiecesEnum.CreaturesAreLands);
			leyWeaverAb.results.Add(ComboPiecesEnum.InfiniteLandUntaps);
			Ability nyleaAb = new Ability("Nylea Ability", true);
			nyleaAb.requirements.Add(ComboPiecesEnum.InfiniteMana);
			nyleaAb.results.Add(ComboPiecesEnum.InfiniteDamage);

			ashaya.Abilities.Add(ashayaAb);
			forest.Abilities.Add(forestAb);
			leyWeaver.Abilities.Add(leyWeaverAb);
			nylea.Abilities.Add(nyleaAb);

			CardLibrary deck = new CardLibrary("Green");
			// deck.AddCard(ashaya);
			deck.commander = ashaya;
			deck.AddCard(forest);
			deck.AddCard(forest.Copy());
			deck.AddCard(forest.Copy());
			deck.AddCard(forest.Copy());
			deck.AddCard(forest.Copy());
			deck.AddCard(leyWeaver);
			deck.AddCard(nylea);
			deck.Shuffle();
			// deck.Print();

			Hand hand = new Hand();
			hand.AddCard(deck.commander);
			hand.ExamineCard(deck.commander);
			foreach (Card card in deck.AllCards)
			{
				hand.AddCard(card);
				hand.ExamineCard(card);
			}

		}
	}
}
