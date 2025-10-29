/*
	MESSAGING
*/

function onClientMessageLegacy( %cl, %msg, %type ) {
	%muted = false;
	%index = String::findSubStr( %msg, "~" );
	if ( %index != -1 ) {
		%tags = String::getSubStr( %msg, %index + 1, 10000 );
		%msg = String::getSubStr( %msg, 0, %index );
	}
	
	if (%client) {
	} else {
		%handled = false;

		if ( String::FindSubStr( %msg, "Station " ) != -1 ) {
			if (%msg == "Station Access On") {
				remoteEnterStation( 2048, $Event::Station );
				%handled = true;
			} else if (%msg == "Station Access Off") {
				remoteExitStation( 2048, $Event::Station );
				$Event::Station = "";
				%handled = true;
			}
		}

		if ( !%handled && ( String::FindSubStr(%msg, " flag") != -1 ) ) {
			%returns = Event::Trigger( eventFlagMessage, %msg );
			%handled = %returns;
		}

		if ( !%handled && ( %msg == "Match started." ) ) {
			remoteMatchStarted( 2048 );
			%handled = true;
		}
	}

    return !%muted;
}

// $Event::Station (station types..)
function remoteINV( %msg ) { 
	if( String::findSubStr( %msg, "STATION ENERGY" ) != -1 ) 
		$Event::Station = "RemoteInventory";
	else 
		$Event::Station = "Inventory";
}

function remoteITXT( %sv, %msg ) {
	Control::setValue(EnergyDisplayText, %msg); 
	remoteINV( %msg );
}

/*
	FLAG EVENTS
*/

Event::Attach( eventFlagMessage, Legacy::flagEvents );

function Legacy::flagEvents( %msg ) {
	if (%msg == "You couldn't buy Flag")
		return "mute";
	
	%event = "";
	%id = 0;
	
	if( ( %idx = String::FindSubStr( %msg, " took the" ) ) != -1 ) {
		%name = String::GetSubStr(%msg, 0, %idx); 
		if ( %name == "You") 
			%name = Client::getName(getManagerId());
		%id = getClientByName( %name );
		%team = Client::GetTeam( %id ) ^ 1;
		%event = "FlagTaken";	
	} else if ( ( %idx = String::FindSubStr( %msg, " dropped the" ) ) != -1 ) {
		%name = String::GetSubStr(%msg, 0, %idx); 
		if ( %name == "You") 
			%name = Client::getName(getManagerId());
		%id = getClientByName( %name );
		%team = Client::GetTeam( %id ) ^ 1;
		%event = "FlagDropped";
	} else if ( ( %idx = String::FindSubStr(%msg, " returned the" ) ) != -1 ) {
		%name = String::GetSubStr(%msg, 0, %idx); 
		if ( %name == "You") 
			%name = Client::getName(getManagerId());
		%id = getClientByName( %name );
		%team = Client::GetTeam( %id );
		%event = "FlagReturned";
	} else if( ( %idx = String::FindSubStr( %msg, " captured the" ) ) != -1 ) {
		%name = String::GetSubStr(%msg, 0, %idx); 
		if ( %name == "You") 
			%name = Client::getName(getManagerId());
		else if ( %name == "Your Team" )
			return;
		%id = getClientByName( %name );
		%team =  Client::GetTeam( %id ) ^ 1;
		%event = "FlagCaptured";
	} else if ( (%idx = String::FindSubStr( %msg, " left the mission" ) ) != -1 ) {
		%name = String::GetSubStr(%msg, 0, %idx); 
		if ( %name == "You") 
			%name = Client::getName(getManagerId());
		%id = getClientByName( %name );
		%team = Client::GetTeam( %id ) ^ 1;
		%event = "FlagReturned";
	}

	if ( %id == 0 || %event == "" )
		return false;
	*"remote"~%event( 2048, %team, %id );
	return true;
}

/*
	KILLS
*/

////////////////////////////////////////////////////////////
// File:	KillTrak.cs
// Version:	3.3b
// Author:	Runar
// Credits:	Daerid
// Modified:Andrew
// Info:	Message parsing killtrack
//
// History:	3.3 Initial Version
//			3.3b Andrew Remix (Fixes Client name exploits)
//
////////////////////////////////////////////////////////////

function KillTrak::DeathMessage(%msg, %weapon) 
{
	//[Andrew]: I've never seen a %4 for a second gender indicator so I stripped
	//the extra message declarations to converse memory

	// Strip out the %1,%2 so we have a base message to compare to, and store a weapon for that message
	$KillTrak::Weapon[sprintf(%msg,"","","his")] = %weapon;
	$KillTrak::Weapon[sprintf(%msg,"","","her")] = %weapon;

	// Check to see the format for this msg
	%pos["%1"] = String::FindSubStr(%msg, "%1");
	%pos["%2"] = String::FindSubStr(%msg, "%2");

	%kfirst = (%pos["%1"] < %pos["%2"]);

	$KillTrak::KillerFirst[sprintf(%msg,"","","his")] = 
	$KillTrak::KillerFirst[sprintf(%msg,"","","her")] = %kfirst;

	%single = (%pos["%1"] == -1);

	$KillTrak::Single[sprintf(%msg,"","","his")] = 
	$KillTrak::Single[sprintf(%msg,"","","her")] = %single;
}

function KillTrak::Parse(%msg)
{
	%names = 0;

	for(%i = 1; %i <= $Team::Client::Count; %i++)
	{
		%name = $Team::Client::Name[ $Team::Client::List[%i] ];

		//[Andrew]: We need to keep track of EVERY player name that is found in the message
		//			e.g. If Bob, Bobber, Bobb, and Bobble are all in the server, there will undoubtedly be
		//				 name clashes.

		//if we found this players name in the msg, add it to the list
   		if((%pos = String::FindSubStr(%msg, %name))!= -1)
		{
			echo("Killtrack: Found possible name: " @ %name);
			%name[%names] = %name;
			%pos[%names] = %pos;
			%names++;
		}
	}
	
	//no names found
	if(%names == 0)
		return;

	%res = false;	
	//single name in the message
	if (%names == 1)
		%res = KillTrak::TestSingle(%msg, %name0);
	//normal 2 names found
	else if (%names == 2)
		%res = KillTrak::TestPair(%msg, %name0, %name1);
	//multiple names found, must test every combination
	else
	{
		//test every name combination, halt on a positive
		for (%i = 0; (%i < %names) && !%res; %i++)
			for (%j = %i+1; (%j < %names) && !%res; %j++)
				{
					echo("Killtrack:: Testing " @ %name[%i] @ " & " @ %name[%j]);
					if (KillTrak::TestPair(%msg, %name[%i], %name[%j]))
						%res = true;
				}
	}
			
	if (%res)
	{
		echo("Kill! Victim: " @ $KillTrak::Victim @ " Killer: " @ $killTrak::Killer @ " Weapon: " @ $killTrak::Weapon);
		remoteKillTrak( 2048, $killTrak::Killer, $KillTrak::Victim, $killTrak::Weapon );
	}
	return ;//mute;
}

function KillTrak::Reset() 
{
	deleteVariables("$KillTrak::*");
}

function KillTrak::TestSingle(%msg, %name)
{
	if (%name == "")
		return false;

	%found = 0;
	%namelen = String::Len(%name[%i]);

	%tmsg = %msg;
	%chopped = 0;

	//find the # of occurances for the name
	while ( (%idx = String::FindSubStr(%tmsg, %name)) != -1 )
	{
		%pos[%found] = %idx + %chopped;	//record the position, modified for the previously chopped names
		%chopped += %namelen;

		//strip this occurance from the test string
		%tmsg = String::GetSubStr(%tmsg, 0, %idx) @ String::GetSubStr(%tmsg, %idx + %namelen, 1024); 
		
		%found++;
	}

	//test each instance of the name for a positive match
	for (%i = 0; %i < %found; %i++)
	{
		%base = String::GetSubStr(%msg, 0, %pos[%i]) @ 
				String::GetSubStr(%msg, %pos[%i] + %namelen, 1024);

		//did this name fit a kill message?
		if ((%wpn = $KillTrak::Weapon[%base]) != "")
		{
			if ($KillTrak::Single[%base])
			{
				//already know this is single
				$KillTrak::Killer = $KillTrak::Victim = getClientByName(%name);
				$KillTrak::Weapon = %wpn;
			
				return true;
			}
		}
	}

	return false;
}

function KillTrak::TestPair(%msg, %name0, %name1)
{
	if ((%name0 == "") && (%name1 != ""))
		return KillTrak::TestSingle(%msg, %name1);
	else if ((%name0 != "") && (%name1 == ""))
		return KillTrak::TestSingle(%msg, %name0);
	else if ((%name0 == "") && (%name1 == ""))
		return false;

	echo("Killtrack: Testing: " @ %name0 @ " & " @ %name1);

	//find the # of occurances for each name
	for (%i = 0; (%i < 2); %i++)
	{
		%found[%i] = 0;
		%namelen[%i] = String::Len(%name[%i]);

		%tmsg = %msg;
		%chopped = 0;
		while ( (%idx = String::FindSubStr(%tmsg, %name[%i])) != -1 )
		{
			%pos[%i, %found[%i]] = %idx + %chopped; //record the position, modified for the previously chopped names
			%chopped += %namelen[%i];

			//strip this occurance from the test string
			%tmsg = String::GetSubStr(%tmsg, 0, %idx) @ String::GetSubStr(%tmsg, %idx + %namelen[%i], 1024);
			
			//increase # found
			%found[%i]++;
		}
	}

	//test each combination of name possibilites
	for (%i = 0; %i < %found[0]; %i++)
	{
		for (%j = 0; %j < %found[1]; %j++)
		{
			//find which name occurance appears first
			if (%pos[0, %i] < %pos[1, %j])
			{
				%first = %pos[0, %i]; 
				%second = %pos[1, %j];
				%firstidx = 0;
				%secondidx = 1;
			}
			else
			{
				%first = %pos[1, %j]; 
				%second = %pos[0, %i];
				%firstidx = 1;
				%secondidx = 0;
			}
			
			//build our test string
			%base = String::GetSubStr(%msg, 0, %first) @ 
					String::GetSubStr(%msg, %first + %namelen[%firstidx], %second - %namelen[%firstidx]) @
					String::GetSubStr(%msg, %second + %namelen[%secondidx], 1024);

			echo("Test string: " @ %base);

			//see if this occurs anywhere
			if ((%wpn = $KillTrak::Weapon[%base]) != "")
			{
				//find who is who
				if ($KillTrak::KillerFirst[%base])
				{
					$KillTrak::Killer = getClientByName(%name[%firstidx]);
					$KillTrak::Victim = getClientByName(%name[%secondidx]);
				}
				else
				{
					$KillTrak::Killer = getClientByName(%name[%secondidx]);
					$KillTrak::Victim = getClientByName(%name[%firstidx]);
				}

				$KillTrak::Weapon = %wpn;
				
				//found it
				return true;
			}
		}
	}

	//didnt, dangit
	return false;
}

//---------------------------------------------------------------------------------
// Player death messages - %1 = killer's name, %2 = victim's name
//		 %3 = killer's gender pronoun (his/her), %4 = victim's gender pronoun
//---------------------------------------------------------------------------------
KillTrak::DeathMessage("%1 falls to %2 death.", "Suicide");
KillTrak::DeathMessage("%1 forgot to tie %2 bungie cord.", "Suicide");
KillTrak::DeathMessage("%1 bites the dust in a forceful manner.", "Suicide");
KillTrak::DeathMessage("%1 thought the ground looked softer.", "Suicide");
KillTrak::DeathMessage("%1 leaves a big ugly crater.", "Suicide");
KillTrak::DeathMessage("%1 makes quite an impact on %2.", "Vehicle");
KillTrak::DeathMessage("%2 becomes the victim of a fly-by from %1.", "Vehicle");
KillTrak::DeathMessage("%2 leaves a nasty dent in %1's fender.", "Vehicle");
KillTrak::DeathMessage("%1 says, 'Hey %2, you scratched my paint job!'", "Vehicle");
KillTrak::DeathMessage("%2 didn't get out of %1's way.", "Vehicle");
KillTrak::DeathMessage("%2 got regulated by %1.", "Chaingun");
KillTrak::DeathMessage("%1 gives %2 an overdose of lead.", "Chaingun");
KillTrak::DeathMessage("%1 fills %2 full of holes.", "Chaingun");
KillTrak::DeathMessage("%1 guns down %2.", "Chaingun");
KillTrak::DeathMessage("%1 gave %2 a hole in %3 head that wasn't there before.", "Chaingun");
KillTrak::DeathMessage("%2 dies of turret trauma.", "Turret");
KillTrak::DeathMessage("%2 is chewed to pieces by a turret.", "Turret");
KillTrak::DeathMessage("%2 walks into a stream of turret fire.", "Turret");
KillTrak::DeathMessage("%2 ends up on the wrong side of a turret.", "Turret");
KillTrak::DeathMessage("%2 finds the business end of a turret.", "Turret");

KillTrak::DeathMessage("%2 feels the warm glow of %1's plasma.", "Plasma");
KillTrak::DeathMessage("%1 gives %2 a white-hot plasma injection.", "Plasma");
KillTrak::DeathMessage("%1 asks %2, 'Got plasma?'", "Plasma");
KillTrak::DeathMessage("%1 gives %2 a burning itch.", "Plasma");
KillTrak::DeathMessage("%2 feels a burning sensation.", "Plasma");

//$TrollPlasmaDamageType -plasmatic
KillTrak::DeathMessage("%2 smelled the warm stench of %1.", "Plasma");
KillTrak::DeathMessage("%1 gives %2 a white-hot plasma injection.", "Plasma");
KillTrak::DeathMessage("%1 asks %2, 'Got Troll?'", "Plasma");
KillTrak::DeathMessage("%1 gives %2 the troll flu.", "Plasma");
KillTrak::DeathMessage("%2 stood down wind of a troll.", "Plasma");

KillTrak::DeathMessage("%2 catches a Frisbee of Death thrown by %1.", "Disc Launcher");
KillTrak::DeathMessage("%1 blasts %2 with a well-placed disc.", "Disc Launcher");
KillTrak::DeathMessage("%1's spinfusor caught %2 by surprise.", "Disc Launcher");
KillTrak::DeathMessage("%2 falls victim to %1's Stormhammer.", "Disc Launcher");
KillTrak::DeathMessage("%1 shows off %3 mad skills of body dismemberment on %2.", "Disc Launcher");
KillTrak::DeathMessage("%1 blows %2 up real good.", "Explosives");
KillTrak::DeathMessage("%2 gets a taste of %1's explosive temper.", "Explosives");
KillTrak::DeathMessage("%1 gives %2 a fatal concussion.", "Explosives");
KillTrak::DeathMessage("%2 never saw it coming from %1.", "Explosives");
KillTrak::DeathMessage("%2 saw the bad side of %1.", "Explosives");


KillTrak::DeathMessage("%1 adds %2 to %3 list of sniper victims.", "Laser Rifle");
KillTrak::DeathMessage("%1 fells %2 with a sniper shot.", "Laser Rifle");
KillTrak::DeathMessage("%2 was assassinated by %1.", "Laser Rifle");
KillTrak::DeathMessage("%2 stayed in %1's crosshairs for too long.", "Laser Rifle");
KillTrak::DeathMessage("%2 gets another hole in %3 head.", "Laser Rifle");
KillTrak::DeathMessage("%1 explodes %2 into oblivion.", "Mortar");
KillTrak::DeathMessage("%2 found %1's silly putty.", "Mortar");
KillTrak::DeathMessage("%1 placed the explosives where %2 could find them.", "Mortar");
KillTrak::DeathMessage("%1's bomb takes out %2.", "Mortar");
KillTrak::DeathMessage("%2 falls all to pieces.", "Mortar");

KillTrak::DeathMessage("%2 gets a blast out of %1.");
KillTrak::DeathMessage("%2 succumbs to %1's rain of blaster fire.");
KillTrak::DeathMessage("%1's puny blaster shows %2 a new world of pain.");
KillTrak::DeathMessage("%2 meets %1's master blaster.");
KillTrak::DeathMessage("%1's punks %2 ghetto style.");
KillTrak::DeathMessage("%2 gets zapped with %1's ELF gun.", "Elf Gun");
KillTrak::DeathMessage("%1 gives %2 a nasty jolt.", "Elf Gun");
KillTrak::DeathMessage("%2 gets a real shock out of meeting %1.", "Elf Gun");
KillTrak::DeathMessage("%1 short-circuits %2's systems.", "Elf Gun");
KillTrak::DeathMessage("%2 is turned to a crispy critter.", "Elf Gun");
//$SoulDamageType
KillTrak::DeathMessage("%2 Gets %4 soul pulled out by %1.", "Elf Gun");
KillTrak::DeathMessage("%1 pulled a rabbit out of %2's head.", "Elf Gun");
KillTrak::DeathMessage("%1 sent %2's soul to Odin.", "Elf Gun");
KillTrak::DeathMessage("%2 dies for %1's sins.", "Elf Gun");
KillTrak::DeathMessage("%1 fried up %2's soul cajun style.", "Elf Gun");
KillTrak::DeathMessage("%2 didn't stay away from the moving parts.", "Crushed");
KillTrak::DeathMessage("%2 is crushed.", "Crushed");
KillTrak::DeathMessage("%2 gets smushed flat.", "Crushed");
KillTrak::DeathMessage("%2 gets caught in the machinery.", "Crushed");
KillTrak::DeathMessage("%2 is sat on by %1's mom.", "Crushed");
KillTrak::DeathMessage("%2 is a victim among the wreckage.", "Explosion");
KillTrak::DeathMessage("%2 is killed by debris.", "Explosion");
KillTrak::DeathMessage("%2 becomes a victim of collateral damage.", "Explosion");
KillTrak::DeathMessage("%2 got too close to the exploding stuff.", "Explosion");
KillTrak::DeathMessage("%2 feels the rain of debris.", "Explosion");
KillTrak::DeathMessage("%2 takes a missile up the keister.", "Missile");
KillTrak::DeathMessage("%2 gets shot down.", "Missile");
KillTrak::DeathMessage("%2 gets real friendly with a rocket.", "Missile");
KillTrak::DeathMessage("%2 feels the burn from a warhead.", "Missile");
KillTrak::DeathMessage("%2 rides the rocket.", "Missile");

//Plasmatic 2.2
KillTrak::DeathMessage("%2 gets a hot rocket injection from %1.", "Explosives");
KillTrak::DeathMessage("%1 gives %2 a lesson in rocketry.", "Explosives");
KillTrak::DeathMessage("%2 polishes %1's rocket.", "Explosives");
KillTrak::DeathMessage("%2 rides %1's big one.", "Explosives");
KillTrak::DeathMessage("%2 failed to outrun %1's rocket.", "Explosives");
//Plasmatic
KillTrak::DeathMessage("%2 gets a hot rocket injection from %1.", "Explosives");
KillTrak::DeathMessage("%1 gives %2 a lesson in rocketry.", "Explosives");
KillTrak::DeathMessage("%2 polishes %1's rocket.", "Explosives");
KillTrak::DeathMessage("%2 rides %1's big one.", "Explosives");
KillTrak::DeathMessage("%2 failed to outrun %1's rocket.", "Explosives");
KillTrak::DeathMessage("%2 should have looked where they were stepping.", "Explosives");
KillTrak::DeathMessage("%2 gets a taste of %1's explosive temper.", "Explosives");
KillTrak::DeathMessage("%1 gives %2 a fatal concussion.", "Explosives");
KillTrak::DeathMessage("%2 never saw it coming from %1.", "Explosives");
KillTrak::DeathMessage("%2 stepped in %1's cow pie. Mooooo!", "Explosives");
KillTrak::DeathMessage("%1 adds %2 to %3 list of sniper victims.", "Laser Rifle");
KillTrak::DeathMessage("%1 fells %2 with a shot between the eyes.", "Laser Rifle");
KillTrak::DeathMessage("%2 was assassinated by %1.", "Laser Rifle");
KillTrak::DeathMessage("%2 stayed in %1's crosshairs for too long.", "Laser Rifle");
KillTrak::DeathMessage("%2 gets picked off from afar by %1.", "Laser Rifle");
KillTrak::DeathMessage("%1 blows %2 up real good.", "Explosives");
KillTrak::DeathMessage("%2 gets a taste of %1's explosive temper.", "Explosives");
KillTrak::DeathMessage("%1 gives %2 a fatal concussion.", "Explosives");
KillTrak::DeathMessage("%2 never saw it coming from %1.", "Explosives");
KillTrak::DeathMessage("%2 gets flashed by %1.", "Explosives");
KillTrak::DeathMessage("%2 caught a chest full of %1's shotgun blast.", "Chaingun");
KillTrak::DeathMessage("%1 filled %2 full of 00-Buckshot.", "Chaingun");
KillTrak::DeathMessage("%1 relieved %2's constipation with a shotgun blast.", "Chaingun");
KillTrak::DeathMessage("%2 gets pelted with %1's rock salt.", "Chaingun");
KillTrak::DeathMessage("%2 gets %3 guts blown out by %1's shotgun.", "Chaingun");
KillTrak::DeathMessage("%2 is assassinated by %1.", "Elf Gun");
KillTrak::DeathMessage("%2 gets %3 throat cut by %1.", "Elf Gun");
KillTrak::DeathMessage("%2 gets stabbed in the back by %1.", "Elf Gun");
KillTrak::DeathMessage("%2 didn't see it coming from %1.", "Elf Gun");
KillTrak::DeathMessage("%1 slices %2's throat.", "Elf Gun");
KillTrak::DeathMessage("%1 removes %2's weapon for the last time.", "Blaster");
KillTrak::DeathMessage("%2 is embarrased by %1.", "Blaster");
//KillTrak::DeathMessage("%2 finds %4self without a gun.");
//KillTrak::DeathMessage("%2 finds %4self without a weapon.");
KillTrak::DeathMessage("%2 is embarrased by %1.", "Blaster");
KillTrak::DeathMessage("%1 told %2 to stand still.", "Blaster");
KillTrak::DeathMessage("%1 told %2 not to move.", "Blaster");
//KillTrak::DeathMessage("%2 is frozen in %4 tracks.");
//KillTrak::DeathMessage("%2 died where %4 stood.");
KillTrak::DeathMessage("%1 plays Mr.Freeze on %2.", "Blaster");
KillTrak::DeathMessage("%2 gets sucked in by %1's turret.", "Blaster");
KillTrak::DeathMessage("%2 is vortexed to another dimension by %1.", "Blaster");
//KillTrak::DeathMessage("%2 finds a better place.");
KillTrak::DeathMessage("%1 sends %2 to a better place.", "Blaster");
KillTrak::DeathMessage("%1 shows %2 a wormhole.", "Blaster");
//KillTrak::DeathMessage("%2 dies of turret trauma.");
//KillTrak::DeathMessage("%2 is chewed to pieces by a turret.");
//KillTrak::DeathMessage("%2 walks into a stream of turret fire.");
//KillTrak::DeathMessage("%2 ends up on the wrong side of a turret.");
//KillTrak::DeathMessage("%2 finds the business end of a turret.");
//KillTrak::DeathMessage("%2 is incarcerated by %1.");
//KillTrak::DeathMessage("%2 is incarcerated by %1.");
//KillTrak::DeathMessage("%2 is incarcerated by %1.");
//KillTrak::DeathMessage("%2 is incarcerated by %1.");
//KillTrak::DeathMessage("%2 is incarcerated by %1.");

// new for 2.2 -PitchFork damage type
KillTrak::DeathMessage("%1 forked %2 off a cliff.", "Vehicle");
KillTrak::DeathMessage("%1 threw %2 against a wall.", "Vehicle");
KillTrak::DeathMessage("%1 won the midget toss with %2.", "Vehicle");
KillTrak::DeathMessage("%1 put %2's head through a wall.", "Vehicle");
KillTrak::DeathMessage("%2 visited a hard object, courtesy of %1.", "Vehicle");

// new for 2.2 -jetting damage type
// Player death messages - %1 = killer's name, %2 = victim's name
//		 %3 = killer's gender pronoun (his/her), %4 = victim's gender pronoun
//KillTrak::DeathMessage("%1 got burned up by %2 jets.");
//KillTrak::DeathMessage("%1's energy system exploded.");
//KillTrak::DeathMessage("%1's jets got turned inside out.");
//KillTrak::DeathMessage("%1 forgot %2 energy system was broken.");
//KillTrak::DeathMessage("%1 was killed by %2 own jump jets.");

// "you just killed yourself" messages
// %1 = player name, %2 = player gender pronoun

KillTrak::DeathMessage("%1 checked into the flatline hotel.", "Suicide");
KillTrak::DeathMessage("%1 takes a dirt nap.", "Suicide");
KillTrak::DeathMessage("%1 kills %2 own dumb self.", "Suicide");
KillTrak::DeathMessage("%1 takes a 6 foot holiday.", "Suicide");	//plas 3.0
KillTrak::DeathMessage("%1 goes for the quick and dirty respawn.", "Suicide");
