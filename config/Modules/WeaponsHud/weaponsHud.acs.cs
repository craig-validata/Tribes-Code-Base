//What a POS this is, but it kinda works, somehow.
//Edited by DaJ4ck3L
//Weapons HUD v1.4 LegendzBeta

Event::Attach(eventConnected, WH::Init);

Event::Attach(eventExitStation, WH::Update);
//Dunno why you have to use a schedule here?
Event::Attach(eventItemReceived, "schedule::add(\"WH::Update();\",0);");

//My events that I added to this pile of junk to make it run smoother
Event::Attach(eventUsedItem, WH::Update);
Event::Attach(eventDropItem, WH::Update);
Event::Attach(EventNextWeapon, WH::Update);
Event::Attach(EventPrevWeapon, WH::Update);
Event::Attach(EventYouFired, WH::Update);

//This exists because I can't get an event for all nec. updates, so run it over and over. 
$WH::UpdateTime = 0.5;

//Helper functions
//Function attachments exist so that you don't have to overwrite them anymore. -Noxwizard
function itemUsed(%desc) after use
{
    %type = getItemType(%desc);
    if (%type != -1)
    {
        Event::Trigger("eventUsedItem", %type);
    }
}

function itemDropped(%desc) after drop
{
    %type = getItemType(%desc);
    if (%type != -1)
    {
        Event::Trigger("eventDropItem", %type);
    }
}

function onNext() before nextWeapon
{
    schedule("checkWeapon(\"Next\", " @ $CheckWepNum++ @ ");", 1);
}

function onPrev() before prevWeapon
{
    schedule("checkWeapon(\"Prev\", " @ $CheckWepNum++ @ ");", 1);
}
//End helpers

function checkWeapon(%type, %num)
{
	if(%num == $CheckWepNum)
    {
		Event::Trigger("event" @ %type @ "Weapon", GetMountedItem(0));
	}
}

function WH::Update()
{
	
	%slot = 0;

	//For all possible weapons
	for (%i=0; %i<=$Weapon::Count; %i++)
	{
		//If the weapon is in your loadout
		if (getItemCount($Weapon::Name[%i]) > 0)
		{
			//Get Ammo
			%ammo = $Weapon::Ammo[%i];
			if (%ammo !="")
				%ammoNum = getItemCount(%ammo);

			//If you are holding the weapon highlight it
			%mounted = false;
			if (getItemType($Weapon::Name[%i]) == getMountedItem(0))
			{
				%mounted = true;
				if($Legendz::SniperActive == false) //zoom snipe stuff -DaJ4ck3L
					$Legendz::prevWpn = $Weapon::Name[%i];
				//Highlight ammo on firing and increase update time
				if ($AF::Firing == "TRUE")
				{
					%ammoNum = "<f2>"~%ammoNum;
					$WH::UpdateTime = 0.1;				
				}
				else
				{
					%ammoNum = "<f1>"~%ammoNum;
					$WH::UpdateTime = 0.5;
				}
			}
			
			//Draw our weapon
			if(%mounted)
				control::setValue("WeaponHUD::Item"~%slot,"<B0,0:modules\\weaponshud\\"~$Weapon::File[%i]~"on.png>");
			else
				control::setValue("WeaponHUD::Item"~%slot,"<B0,0:modules\\weaponshud\\"~$Weapon::File[%i]~".png>");

			//Draw our ammo
			control::setValue("WeaponHUD::Ammo"~%slot,%ammoNum);

			//Go to the next slut, err slot
			%ammoNum = "";
			%slot++;
		}
	}

	//Clear the rest of the weapon slots
	for (%slot; %slot<=10;%slot++)
	{
		control::setValue("WeaponHUD::Item"~%slot,"");
		control::setValue("WeaponHUD::Ammo"~%slot,"");

	}

	//Fuck I want to get rid of this. Someone help!
	schedule::add("WH::Update();", $WH::UpdateTime);

}

//Add our items
function WH::AddItem(%item, %file, %ammo) {
	if(getItemType(%item) != -1) {
		//echo("Added "~%item);

		//ID num for weapon
		%num = getItemType(%item);
		$Weapon::Num[$Weapon::Count] = %num;

		//Name of Weapon
		$Weapon::Name[$Weapon::Count] = %item;

		//Filename for image
		$Weapon::File[$Weapon::Count] = %file;

		//Ammo Name for weapon
		$Weapon::Ammo[$Weapon::Count] = %ammo;

		//++ baby!
		$Weapon::Count++;

		return true;
	}
	return false;
}

//Crappy bubble sort, short list, who cares, only runs on init anyway
function WH::Sort()
{
	for(%i=0;%i<$Weapon::Count;%i++)
	{
		for (%j=1;%j<$Weapon::Count;%j++)
		{
			if ($Weapon::Num[%j] < $Weapon::Num[%j-1])
				WH::Swap(%j,%j-1);
		}
	}
}
			
//Swap two weapons	
function WH::Swap(%one,%two)
{
		%temp[0] = $Weapon::Num[%one];
		%temp[1] = $Weapon::Name[%one];
		%temp[2] = $Weapon::File[%one];
		%temp[3] = $Weapon::Ammo[%one];

		$Weapon::Num[%one] = $Weapon::Num[%two];
		$Weapon::Name[%one] = $Weapon::Name[%two];
		$Weapon::File[%one] = $Weapon::File[%two];
		$Weapon::Ammo[%one] = $Weapon::Ammo[%two];

		$Weapon::Num[%two] = %temp[0];
		$Weapon::Name[%two] = %temp[1];
		$Weapon::File[%two] = %temp[2];
		$Weapon::Ammo[%two] = %temp[3];
}

//Wake Sleep shit
function WH::Wake() {
	$WH::Awake = true;
	WH::Update();
}

function WH::Sleep() {
	Schedule::Cancel("WH::Update();");
	$WH::Awake = false;
}

function WH::Create() {

	if ($WeaponHUD::Loaded)
		return;
	$WeaponHUD::Loaded = true;

	$WeaponHUD::Awake = false;

	HUD::New( "WeaponHUD::Container", 0, 0, 710, 70, WH::Wake, WH::Sleep );

	//11 slots should be enough, any mod that has more than 11 can suck my ass
	for (%i=0;%i<=10;%i++)
	{
		newObject( "WeaponHUD::Item"~%i, FearGuiFormattedText, 0+(70*%i), 5, 70, 35 );
		newObject( "WeaponHUD::Ammo"~%i, FearGuiFormattedText, 29+(70*%i), 25, 70, 35 );

		HUD::Add( "WeaponHUD::Container", "WeaponHUD::Item"~%i );
		HUD::Add( "WeaponHUD::Container", "WeaponHUD::Ammo"~%i );

	}
}


function WH::Init() {

    $CheckWepNum = 0;

	//Clear out current variables
	DeleteVariables("$Weapon::*");
	$Weapon::Count = 0;
	
	//lets split this up by mods !!! -DaJ4ck3L
	if($ServerMod == "Annihilation")
	{
		//Weapons
		WH::AddItem("Grenade Launcher X", "grenade", "Grenade Ammo X");
		WH::AddItem("Plasma Gun X", "plasma", "PlasmaBolt X");
		WH::AddItem("Disc Launcher X", "disc", "Disc X"); //shiat wish i freaking changed the weapon name now lol
		//New Arena Guns
		WH::AddItem("Plasma Gun Elite", "plasma", "PlasmaBolt Elite");
		WH::AddItem("Disc Launcher Elite", "disc", "Disc Elite");
		WH::AddItem("Plasma Gun Base", "plasma", "PlasmaBolt Base");
		WH::AddItem("Disc Launcher Base", "disc", "Disc Base");
		//end
		WH::AddItem("Plasma Gun", "plasma", "PlasmaBolt");
		WH::AddItem("Grappling Hook", "target");
		WH::AddItem("Buckler", "shieldimgx");
		WH::AddItem("X72 Gravity Gun", "target");
		WH::AddItem("Builder Gun", "mortar");
		WH::AddItem("Blaster","blaster");
		WH::AddItem("ChainGun", "chaingun", "Bullet");
		WH::AddItem("Disc Launcher", "disc", "Disc");
		WH::AddItem("Elf Gun", "elf");
		WH::AddItem("Grenade Launcher", "grenade", "Grenade Ammo");
		WH::AddItem("Laser Rifle", "sniper");
		WH::AddItem("Minigun", "minigun");
		WH::AddItem("Thumper", "thumper", "Thumper Ammo");
		WH::AddItem("Mortar", "mortar", "Mortar Ammo");
		WH::AddItem("Angel Fire", "energyshot");
		WH::AddItem("Bio Feedback Beam", "target");
		WH::AddItem("Baby Nuke Launcher", "mortar", "Nuclear Device");
		WH::AddItem("Cutting Laser", "blaster");
		WH::AddItem("Builder Repair-Gun", "repairpack");
		WH::AddItem("Flamer", "elf", "FlamerAmmo");
		WH::AddItem("Flame Thrower", "mortarammo", "Flame Cartridges");
		WH::AddItem("Mini Bomber", "grenade", "Mini Ammo");
		WH::AddItem("Lucifers Hammer", "mortarammo", "Hammer Ammo");
		WH::AddItem("Hyper Spinfusor", "disc", "Hyper Disc");
		WH::AddItem("Heavens Gate", "target");
		WH::AddItem("Heaven's Fury", "elf");
		WH::AddItem("Jailers Gun", "elf"); //Clay Pigeon Launcher
		WH::AddItem("Clay Pigeon Launcher", "grenade", "ClayPigeon Can"); //new/old gun doh -DaJ4ck3L
		WH::AddItem("OS Launcher", "grenade", "OS Rockets");
		WH::AddItem("OS Laucher", "grenade", "OS Rockets"); //lol wonder who miss spelled this -DaJ4ck3L
		WH::AddItem("Particle Beam", "sexypbeam");
		WH::AddItem("Phase Disrupter", "blaster", "Disrupter Ammo");
		WH::AddItem("Pitchfork", "mortar");
		WH::AddItem("Railgun", "sniper", "Railgun Bolt");
		WH::AddItem("Rocket Launcher", "mortar", "Rockets");
		WH::AddItem("Rocket Pod", "mortar", "Pod Rockets");
		WH::AddItem("Rubbery Mortar", "mortar", "R Mortar Ammo");
		WH::AddItem("Shockwave Cannon", "elf");
		WH::AddItem("Shotgun", "elf", "Shotgun Shells");
		WH::AddItem("Sniper Rifle", "sniper",  "Sniper Ammo");
		WH::AddItem("Soul Grabber", "mortarammo");
		WH::AddItem("Spell: Death Ray", "plasmaturret");
		WH::AddItem("Spell: Disarm", "mortartrail");
		WH::AddItem("Spell: Flame Strike", "plasmaammo");
		WH::AddItem("Spell: Flame Thrower", "plasmaammo");
		WH::AddItem("Spell: Shocking Grasp", "plant");
		WH::AddItem("Spell: Stasis", "energyshot");
		WH::AddItem("Stinger Missle", "grenade", "Stinger Ammo");
		WH::AddItem("Tank Blast Cannon", "mortar", "Blast Cannon Shots");
		WH::AddItem("Tank Rocketgun", "mortar", "TRocketLauncher Ammo");
		WH::AddItem("Tank RPG", "mortar", "RPG Ammo");
		WH::AddItem("Tank Shredder", "chaingun", "Tank Shredder Ammo");
		WH::AddItem("Vulcan", "chaingun", "Vulcan Bullet");
		WH::AddItem("Slapper", "sniper");
		WH::AddItem("Energy Blade", "energyshot");
		
		//Turrets
		WH::AddItem("Flame Turret", "turret");
		WH::AddItem("Fusion Turret", "turret");
		WH::AddItem("Ion Turret", "turret");
		WH::AddItem("Remote Ion Turret", "turret");
		WH::AddItem("Irradiation Cannon", "turret");
		WH::AddItem("Laser Turret", "camera");
		WH::AddItem("Missile Turret", "turret");
		WH::AddItem("Mortar Turret", "turret");
		WH::AddItem("Neuro Basher", "turret");
		WH::AddItem("Nuclear Turret", "turret");
		WH::AddItem("Particle Beam Turret", "turret");
		WH::AddItem("Plasma Turret", "turret");
		WH::AddItem("Shock Turret", "turret");
		WH::AddItem("Vortex Turret", "camera");
		WH::AddItem("Death Star", "turret");
		
		//Deploys
		WH::AddItem("Air Base", "ammopack");
		WH::AddItem("Base Cloaking Device", "ammopack");
		WH::AddItem("Big Crate", "ammopack");
		WH::AddItem("Blast Wall", "ammopack");
		WH::AddItem("Deployable Bunker", "ammopack");
		WH::AddItem("Command Station", "ammostation");
		WH::AddItem("Control Jammer", "ammopack");
		WH::AddItem("Transport Vehicle", "ammostation");
		WH::AddItem("Fighter Vehicle Pack", "ammostation");
		WH::AddItem("Force Field", "ammopack");
		WH::AddItem("Force Field Door", "ammopack");
		WH::AddItem("Jail Tower", "shieldpack");
		WH::AddItem("Jump Pad", "ammopack");
		WH::AddItem("Large Force Field", "ammopack");
		WH::AddItem("Large Force Field Door", "ammopack");
		WH::AddItem("Mobile Inventory", "inventory");
		WH::AddItem("Deployable Platform", "ammopack");
		WH::AddItem("Portable Generator", "inventory");
		WH::AddItem("Portable Solar Panel", "inventory");
		WH::AddItem("Cat sensor", "shieldpack");
		WH::AddItem("Teleport Pad", "sensorjammer");
		WH::AddItem("Command Ship Beacon", "motionsensor");
		WH::AddItem("Gun Ship Beacon", "motionsensor");
		WH::AddItem("Supply Ship Beacon", "motionsensor");
		WH::AddItem("Inventory Station", "inventory");
		WH::AddItem("Camera" ,"camera");
		WH::AddItem("Sensor Jammer", "sensorjammer");
		WH::AddItem("Ammo Station", "ammostation");
		WH::AddItem("Motion Sensor","motionsensor");
		WH::AddItem("Airstrike beacon","motionsensor");
		
		//Packs
		WH::AddItem("Ammo Pack", "ammopack");
		WH::AddItem("Energy Pack" ,"energypack");
		WH::AddItem("Repair Gun", "repairpack");
		WH::AddItem("Pulse Sensor", "pulse");
		WH::AddItem("Repair Pack" ,"repairpack");
		WH::AddItem("Shield Pack" ,"shieldpack");
		WH::AddItem("Sensor Jammer Pack", "sensorjammerpack");
		WH::AddItem("Turret", "turret");
		WH::AddItem("Chameleon Pack", "ammopack");
		WH::AddItem("Nova Pack", "ammostation");
		WH::AddItem("Cloaking Device", "sensorjammerpack");
		WH::AddItem("Laptop", "ammostation");
		WH::AddItem("Ghost Pack", "sensorjammerpack");
		WH::AddItem("Phase Shifter", "shieldpack");
		WH::AddItem("Troll Pack", "ammopack");
		WH::AddItem("StealthShield Pack", "shieldpack");
		WH::AddItem("Suicide DetPack", "motionsensor");
		WH::AddItem("Tank Pack", "turret");
		WH::AddItem("True Sight Pack", "sensorjammerpack");
		WH::AddItem("Survey Droid", "camera");
		WH::AddItem("Probe Droid", "camera");
		WH::AddItem("Suicide Droid", "camera");
	}
	else if($ServerMod == "Annihilation Legendz")
	{
		//Weapons
		WH::AddItem("Grenade Launcher X", "grenade", "Grenade Ammo X");
		WH::AddItem("Plasma Gun X", "plasma", "PlasmaBolt X");
		WH::AddItem("Disc Launcher X", "disc", "Disc X"); //shiat wish i freaking changed the weapon name now lol
		WH::AddItem("Plasma Gun", "plasma", "PlasmaBolt");
		WH::AddItem("Grappling Hook", "shieldimgx");
		WH::AddItem("Buckler", "target");
		WH::AddItem("X72 Gravity Gun", "target");
		WH::AddItem("Builder Gun", "mortar");
		WH::AddItem("Blaster","blaster");
		WH::AddItem("ChainGun", "chaingun", "Bullet");
		WH::AddItem("Disc Launcher", "disc", "Disc");
		WH::AddItem("Elf Gun", "elf");
		WH::AddItem("Grenade Launcher", "grenade", "Grenade Ammo");
		WH::AddItem("Laser Rifle", "sniper");
		WH::AddItem("Mortar", "mortar", "Mortar Ammo");
		WH::AddItem("Angel Fire", "energyshot");
		WH::AddItem("Baby Nuke Launcher", "mortar", "Nuclear Device");
		WH::AddItem("Cutting Laser", "blaster");
		WH::AddItem("Builder Repair-Gun", "repairpack");
		WH::AddItem("Flamer", "elf", "FlamerAmmo");
		WH::AddItem("Flame Thrower", "mortarammo", "Flame Cartridges");
		WH::AddItem("Mini Bomber", "grenade", "Mini Ammo");
		WH::AddItem("Lucifers Hammer", "mortarammo", "Hammer Ammo");
		WH::AddItem("Hyper Spinfusor", "disc", "Hyper Disc");
		WH::AddItem("Heavens Gate", "target");
		WH::AddItem("Heaven's Fury", "elf");
		WH::AddItem("Jailers Gun", "elf");
		WH::AddItem("Clay Pigeon Launcher", "grenade", "ClayPigeon Can"); //new/old gun doh -DaJ4ck3L
		WH::AddItem("OS Launcher", "grenade", "OS Rockets");
		WH::AddItem("OS Laucher", "grenade", "OS Rockets"); //lol wonder who miss spelled this -DaJ4ck3L
		WH::AddItem("Particle Beam", "sexypbeam");
		WH::AddItem("Phase Disrupter", "blaster", "Disrupter Ammo");
		WH::AddItem("Pitchfork", "mortar");
		WH::AddItem("Railgun", "sniper", "Railgun Bolt");
		WH::AddItem("Rocket Launcher", "mortar", "Rockets");
		WH::AddItem("Rocket Pod", "mortar", "Pod Rockets");
		WH::AddItem("Rubbery Mortar", "mortar", "R Mortar Ammo");
		WH::AddItem("Shockwave Cannon", "elf");
		WH::AddItem("Shotgun", "elf", "Shotgun Shells");
		WH::AddItem("Sniper Rifle", "sniper",  "Sniper Ammo");
		WH::AddItem("Soul Grabber", "mortarammo");
		WH::AddItem("Spell: Death Ray", "plasmaturret");
		WH::AddItem("Spell: Disarm", "mortartrail");
		WH::AddItem("Spell: Flame Strike", "plasmaammo");
		WH::AddItem("Spell: Flame Thrower", "plasmaammo");
		WH::AddItem("Spell: Shocking Grasp", "plant");
		WH::AddItem("Spell: Stasis", "energyshot");
		WH::AddItem("Stinger Missle", "grenade", "Stinger Ammo");
		WH::AddItem("Tank Blast Cannon", "mortar", "Blast Cannon Shots");
		WH::AddItem("Tank Rocketgun", "mortar", "TRocketLauncher Ammo");
		WH::AddItem("Tank RPG", "mortar", "RPG Ammo");
		WH::AddItem("Tank Shredder", "chaingun", "Tank Shredder Ammo");
		WH::AddItem("Vulcan", "chaingun", "Vulcan Bullet");
		WH::AddItem("Slapper", "sniper");
		
		//Turrets
		WH::AddItem("Flame Turret", "turret");
		WH::AddItem("Fusion Turret", "turret");
		WH::AddItem("Ion Turret", "turret");
		WH::AddItem("Remote Ion Turret", "turret");
		WH::AddItem("Irradiation Cannon", "turret");
		WH::AddItem("Laser Turret", "camera");
		WH::AddItem("Missile Turret", "turret");
		WH::AddItem("Mortar Turret", "turret");
		WH::AddItem("Neuro Basher", "turret");
		WH::AddItem("Nuclear Turret", "turret");
		WH::AddItem("Particle Beam Turret", "turret");
		WH::AddItem("Plasma Turret", "turret");
		WH::AddItem("Shock Turret", "turret");
		WH::AddItem("Vortex Turret", "camera");
		
		//Deploys
		WH::AddItem("Air Base", "ammopack");
		WH::AddItem("Base Cloaking Device", "ammopack");
		WH::AddItem("Big Crate", "ammopack");
		WH::AddItem("Blast Wall", "ammopack");
		WH::AddItem("Deployable Bunker", "ammopack");
		WH::AddItem("Command Station", "ammostation");
		WH::AddItem("Control Jammer", "ammopack");
		WH::AddItem("Transport Vehicle", "ammostation");
		WH::AddItem("Fighter Vehicle Pack", "ammostation");
		WH::AddItem("Force Field", "ammopack");
		WH::AddItem("Force Field Door", "ammopack");
		WH::AddItem("Jail Tower", "shieldpack");
		WH::AddItem("Jump Pad", "ammopack");
		WH::AddItem("Large Force Field", "ammopack");
		WH::AddItem("Large Force Field Door", "ammopack");
		WH::AddItem("Mobile Inventory", "inventory");
		WH::AddItem("Deployable Platform", "ammopack");
		WH::AddItem("Portable Generator", "inventory");
		WH::AddItem("Portable Solar Panel", "inventory");
		WH::AddItem("Cat sensor", "shieldpack");
		WH::AddItem("Teleport Pad", "sensorjammer");
		WH::AddItem("Command Ship Beacon", "motionsensor");
		WH::AddItem("Gun Ship Beacon", "motionsensor");
		WH::AddItem("Supply Ship Beacon", "motionsensor");
		WH::AddItem("Inventory Station", "inventory");
		WH::AddItem("Camera" ,"camera");
		WH::AddItem("Sensor Jammer", "sensorjammer");
		WH::AddItem("Ammo Station", "ammostation");
		WH::AddItem("Ammo Pack", "ammopack");
		WH::AddItem("Motion Sensor","motionsensor");
		
		//Packs
		WH::AddItem("Energy Pack" ,"energypack");
		WH::AddItem("Repair Gun", "repairpack");
		WH::AddItem("Pulse Sensor", "pulse");
		WH::AddItem("Repair Pack" ,"repairpack");
		WH::AddItem("Shield Pack" ,"shieldpack");
		WH::AddItem("Sensor Jammer Pack", "sensorjammerpack");
		WH::AddItem("Turret", "turret");
		WH::AddItem("Chameleon Pack", "ammopack");
		WH::AddItem("Cloaking Device", "sensorjammerpack");
		WH::AddItem("Laptop", "ammostation");
		WH::AddItem("Ghost Pack", "sensorjammerpack");
		WH::AddItem("Phase Shifter", "shieldpack");
		WH::AddItem("Troll Pack", "ammopack");
		WH::AddItem("StealthShield Pack", "shieldpack");
		WH::AddItem("Suicide DetPack", "motionsensor");
		WH::AddItem("Tank Pack", "turret");
		WH::AddItem("True Sight Pack", "sensorjammerpack");
		WH::AddItem("Survey Droid", "camera");
		WH::AddItem("Probe Droid", "camera");
		WH::AddItem("Suicide Droid", "camera");
	}
	else if($ServerMod == "Annihilation Legendz v6.0R")
	{
		//Weapons
		WH::AddItem("Grenade Launcher X", "grenade", "Grenade Ammo X");
		WH::AddItem("Plasm Gun X", "plasma", "PlasmaBolt X");
		WH::AddItem("Disc Launcher X", "disc", "Disc X"); //shiat wish i freaking changed the weapon name now lol
		WH::AddItem("Plasma Gun", "plasma", "PlasmaBolt");
		WH::AddItem("Grappling Hook", "shieldimgx");
		WH::AddItem("Buckler", "target");
		WH::AddItem("X72 Gravity Gun", "target");
		WH::AddItem("Builder Gun", "mortar");
		WH::AddItem("Blaster","blaster");
		WH::AddItem("ChainGun", "chaingun", "Bullet");
		WH::AddItem("Disc Launcher", "disc", "Disc");
		WH::AddItem("Elf Gun", "elf");
		WH::AddItem("Grenade Launcher", "grenade", "Grenade Ammo");
		WH::AddItem("Laser Rifle", "sniper");
		WH::AddItem("Mortar", "mortar", "Mortar Ammo");
		WH::AddItem("Angel Fire", "energyshot");
		WH::AddItem("Baby Nuke Launcher", "mortar", "Nuclear Device");
		WH::AddItem("Cutting Laser", "blaster");
		WH::AddItem("Builder Repair-Gun", "repairpack");
		WH::AddItem("Flamer", "elf", "FlamerAmmo");
		WH::AddItem("Flame Thrower", "mortarammo", "Flame Cartridges");
		WH::AddItem("Mini Bomber", "grenade", "Mini Ammo");
		WH::AddItem("Lucifers Hammer", "mortarammo", "Hammer Ammo");
		WH::AddItem("Hyper Spinfusor", "disc", "Hyper Disc");
		WH::AddItem("Heavens Gate", "target");
		WH::AddItem("Heaven's Fury", "elf");
		WH::AddItem("Jailers Gun", "elf");
		WH::AddItem("Clay Pigeon Launcher", "grenade", "ClayPigeon Can"); //new/old gun doh -DaJ4ck3L
		WH::AddItem("OS Launcher", "grenade", "OS Rockets");
		WH::AddItem("OS Laucher", "grenade", "OS Rockets"); //lol wonder who miss spelled this -DaJ4ck3L
		WH::AddItem("Particle Beam", "sexypbeam");
		WH::AddItem("Phase Disrupter", "blaster", "Disrupter Ammo");
		WH::AddItem("Pitchfork", "mortar");
		WH::AddItem("Railgun", "sniper", "Railgun Bolt");
		WH::AddItem("Rocket Launcher", "mortar", "Rockets");
		WH::AddItem("Rocket Pod", "mortar", "Pod Rockets");
		WH::AddItem("Rubbery Mortar", "mortar", "R Mortar Ammo");
		WH::AddItem("Shockwave Cannon", "elf");
		WH::AddItem("Shotgun", "elf", "Shotgun Shells");
		WH::AddItem("Sniper Rifle", "sniper",  "Sniper Ammo");
		WH::AddItem("Soul Grabber", "mortarammo");
		WH::AddItem("Spell: Death Ray", "plasmaturret");
		WH::AddItem("Spell: Disarm", "mortartrail");
		WH::AddItem("Spell: Flame Strike", "plasmaammo");
		WH::AddItem("Spell: Flame Thrower", "plasmaammo");
		WH::AddItem("Spell: Shocking Grasp", "plant");
		WH::AddItem("Spell: Stasis", "energyshot");
		WH::AddItem("Stinger Missle", "grenade", "Stinger Ammo");
		WH::AddItem("Tank Blast Cannon", "mortar", "Blast Cannon Shots");
		WH::AddItem("Tank Rocketgun", "mortar", "TRocketLauncher Ammo");
		WH::AddItem("Tank RPG", "mortar", "RPG Ammo");
		WH::AddItem("Tank Shredder", "chaingun", "Tank Shredder Ammo");
		WH::AddItem("Vulcan", "chaingun", "Vulcan Bullet");
		WH::AddItem("Slapper", "sniper");
		
		//Turrets
		WH::AddItem("Flame Turret", "turret");
		WH::AddItem("Fusion Turret", "turret");
		WH::AddItem("Ion Turret", "turret");
		WH::AddItem("Remote Ion Turret", "turret");
		WH::AddItem("Irradiation Cannon", "turret");
		WH::AddItem("Laser Turret", "camera");
		WH::AddItem("Missile Turret", "turret");
		WH::AddItem("Mortar Turret", "turret");
		WH::AddItem("Neuro Basher", "turret");
		WH::AddItem("Nuclear Turret", "turret");
		WH::AddItem("Particle Beam Turret", "turret");
		WH::AddItem("Plasma Turret", "turret");
		WH::AddItem("Shock Turret", "turret");
		WH::AddItem("Vortex Turret", "camera");
		
		//Deploys
		WH::AddItem("Air Base", "ammopack");
		WH::AddItem("Base Cloaking Device", "ammopack");
		WH::AddItem("Big Crate", "ammopack");
		WH::AddItem("Blast Wall", "ammopack");
		WH::AddItem("Deployable Bunker", "ammopack");
		WH::AddItem("Command Station", "ammostation");
		WH::AddItem("Control Jammer", "ammopack");
		WH::AddItem("Transport Vehicle", "ammostation");
		WH::AddItem("Fighter Vehicle Pack", "ammostation");
		WH::AddItem("Force Field", "ammopack");
		WH::AddItem("Force Field Door", "ammopack");
		WH::AddItem("Jail Tower", "shieldpack");
		WH::AddItem("Jump Pad", "ammopack");
		WH::AddItem("Large Force Field", "ammopack");
		WH::AddItem("Large Force Field Door", "ammopack");
		WH::AddItem("Mobile Inventory", "inventory");
		WH::AddItem("Deployable Platform", "ammopack");
		WH::AddItem("Portable Generator", "inventory");
		WH::AddItem("Portable Solar Panel", "inventory");
		WH::AddItem("Cat sensor", "shieldpack");
		WH::AddItem("Teleport Pad", "sensorjammer");
		WH::AddItem("Command Ship Beacon", "motionsensor");
		WH::AddItem("Gun Ship Beacon", "motionsensor");
		WH::AddItem("Supply Ship Beacon", "motionsensor");
		WH::AddItem("Inventory Station", "inventory");
		WH::AddItem("Camera" ,"camera");
		WH::AddItem("Sensor Jammer", "sensorjammer");
		WH::AddItem("Ammo Station", "ammostation");
		WH::AddItem("Ammo Pack", "ammopack");
		WH::AddItem("Motion Sensor","motionsensor");
		
		//Packs
		WH::AddItem("Energy Pack" ,"energypack");
		WH::AddItem("Repair Gun", "repairpack");
		WH::AddItem("Pulse Sensor", "pulse");
		WH::AddItem("Repair Pack" ,"repairpack");
		WH::AddItem("Shield Pack" ,"shieldpack");
		WH::AddItem("Sensor Jammer Pack", "sensorjammerpack");
		WH::AddItem("Turret", "turret");
		WH::AddItem("Chameleon Pack", "ammopack");
		WH::AddItem("Cloaking Device", "sensorjammerpack");
		WH::AddItem("Laptop", "ammostation");
		WH::AddItem("Ghost Pack", "sensorjammerpack");
		WH::AddItem("Phase Shifter", "shieldpack");
		WH::AddItem("Troll Pack", "ammopack");
		WH::AddItem("StealthShield Pack", "shieldpack");
		WH::AddItem("Suicide DetPack", "motionsensor");
		WH::AddItem("Tank Pack", "turret");
		WH::AddItem("True Sight Pack", "sensorjammerpack");
		WH::AddItem("Survey Droid", "camera");
		WH::AddItem("Probe Droid", "camera");
		WH::AddItem("Suicide Droid", "camera");
	}
	else if($ServerMod == "Annihilation Legendz v6.0")
	{
		//Weapons
		WH::AddItem("Grenade Launcher X", "grenade", "Grenade Ammo X");
		WH::AddItem("Plasm Gun X", "plasma", "PlasmaBolt X");
		WH::AddItem("Disc Launcher X", "disc", "Disc X"); //shiat wish i freaking changed the weapon name now lol
		WH::AddItem("Plasma Gun", "plasma", "PlasmaBolt");
		WH::AddItem("Grappling Hook", "shieldimgx");
		WH::AddItem("Buckler", "target");
		WH::AddItem("X72 Gravity Gun", "target");
		WH::AddItem("Builder Gun", "mortar");
		WH::AddItem("Blaster","blaster");
		WH::AddItem("ChainGun", "chaingun", "Bullet");
		WH::AddItem("Disc Launcher", "disc", "Disc");
		WH::AddItem("Elf Gun", "elf");
		WH::AddItem("Grenade Launcher", "grenade", "Grenade Ammo");
		WH::AddItem("Laser Rifle", "sniper");
		WH::AddItem("Minigun", "minigun");
		WH::AddItem("Thumper", "thumper", "Thumper Ammo");
		WH::AddItem("Mortar", "mortar", "Mortar Ammo");
		WH::AddItem("Angel Fire", "energyshot");
		WH::AddItem("Baby Nuke Launcher", "mortar", "Nuclear Device");
		WH::AddItem("Cutting Laser", "blaster");
		WH::AddItem("Builder Repair-Gun", "repairpack");
		WH::AddItem("Flamer", "elf", "FlamerAmmo");
		WH::AddItem("Flame Thrower", "mortarammo", "Flame Cartridges");
		WH::AddItem("Mini Bomber", "grenade", "Mini Ammo");
		WH::AddItem("Lucifers Hammer", "mortarammo", "Hammer Ammo");
		WH::AddItem("Hyper Spinfusor", "disc", "Hyper Disc");
		WH::AddItem("Heavens Gate", "target");
		WH::AddItem("Heaven's Fury", "elf");
		WH::AddItem("Jailers Gun", "elf");
		WH::AddItem("Clay Pigeon Launcher", "grenade", "ClayPigeon Can"); //new/old gun doh -DaJ4ck3L
		WH::AddItem("OS Launcher", "grenade", "OS Rockets");
		WH::AddItem("OS Laucher", "grenade", "OS Rockets"); //lol wonder who miss spelled this -DaJ4ck3L
		WH::AddItem("Particle Beam", "sexypbeam");
		WH::AddItem("Phase Disrupter", "blaster", "Disrupter Ammo");
		WH::AddItem("Pitchfork", "mortar");
		WH::AddItem("Railgun", "sniper", "Railgun Bolt");
		WH::AddItem("Rocket Launcher", "mortar", "Rockets");
		WH::AddItem("Rocket Pod", "mortar", "Pod Rockets");
		WH::AddItem("Rubbery Mortar", "mortar", "R Mortar Ammo");
		WH::AddItem("Shockwave Cannon", "elf");
		WH::AddItem("Shotgun", "elf", "Shotgun Shells");
		WH::AddItem("Sniper Rifle", "sniper",  "Sniper Ammo");
		WH::AddItem("Soul Grabber", "mortarammo");
		WH::AddItem("Spell: Death Ray", "plasmaturret");
		WH::AddItem("Spell: Disarm", "mortartrail");
		WH::AddItem("Spell: Flame Strike", "plasmaammo");
		WH::AddItem("Spell: Flame Thrower", "plasmaammo");
		WH::AddItem("Spell: Shocking Grasp", "plant");
		WH::AddItem("Spell: Stasis", "energyshot");
		WH::AddItem("Stinger Missle", "grenade", "Stinger Ammo");
		WH::AddItem("Tank Blast Cannon", "mortar", "Blast Cannon Shots");
		WH::AddItem("Tank Rocketgun", "mortar", "TRocketLauncher Ammo");
		WH::AddItem("Tank RPG", "mortar", "RPG Ammo");
		WH::AddItem("Tank Shredder", "chaingun", "Tank Shredder Ammo");
		WH::AddItem("Vulcan", "chaingun", "Vulcan Bullet");
		WH::AddItem("Slapper", "sniper");
		
		//Turrets
		WH::AddItem("Flame Turret", "turret");
		WH::AddItem("Fusion Turret", "turret");
		WH::AddItem("Ion Turret", "turret");
		WH::AddItem("Remote Ion Turret", "turret");
		WH::AddItem("Irradiation Cannon", "turret");
		WH::AddItem("Laser Turret", "camera");
		WH::AddItem("Missile Turret", "turret");
		WH::AddItem("Mortar Turret", "turret");
		WH::AddItem("Neuro Basher", "turret");
		WH::AddItem("Nuclear Turret", "turret");
		WH::AddItem("Particle Beam Turret", "turret");
		WH::AddItem("Plasma Turret", "turret");
		WH::AddItem("Shock Turret", "turret");
		WH::AddItem("Vortex Turret", "camera");
		
		//Deploys
		WH::AddItem("Air Base", "ammopack");
		WH::AddItem("Base Cloaking Device", "ammopack");
		WH::AddItem("Big Crate", "ammopack");
		WH::AddItem("Blast Wall", "ammopack");
		WH::AddItem("Deployable Bunker", "ammopack");
		WH::AddItem("Command Station", "ammostation");
		WH::AddItem("Control Jammer", "ammopack");
		WH::AddItem("Transport Vehicle", "ammostation");
		WH::AddItem("Fighter Vehicle Pack", "ammostation");
		WH::AddItem("Force Field", "ammopack");
		WH::AddItem("Force Field Door", "ammopack");
		WH::AddItem("Jail Tower", "shieldpack");
		WH::AddItem("Jump Pad", "ammopack");
		WH::AddItem("Large Force Field", "ammopack");
		WH::AddItem("Large Force Field Door", "ammopack");
		WH::AddItem("Mobile Inventory", "inventory");
		WH::AddItem("Deployable Platform", "ammopack");
		WH::AddItem("Portable Generator", "inventory");
		WH::AddItem("Portable Solar Panel", "inventory");
		WH::AddItem("Cat sensor", "shieldpack");
		WH::AddItem("Teleport Pad", "sensorjammer");
		WH::AddItem("Command Ship Beacon", "motionsensor");
		WH::AddItem("Gun Ship Beacon", "motionsensor");
		WH::AddItem("Supply Ship Beacon", "motionsensor");
		WH::AddItem("Inventory Station", "inventory");
		WH::AddItem("Camera" ,"camera");
		WH::AddItem("Sensor Jammer", "sensorjammer");
		WH::AddItem("Ammo Station", "ammostation");
		WH::AddItem("Ammo Pack", "ammopack");
		WH::AddItem("Motion Sensor","motionsensor");
		
		//Packs
		WH::AddItem("Energy Pack" ,"energypack");
		WH::AddItem("Repair Gun", "repairpack");
		WH::AddItem("Pulse Sensor", "pulse");
		WH::AddItem("Repair Pack" ,"repairpack");
		WH::AddItem("Shield Pack" ,"shieldpack");
		WH::AddItem("Sensor Jammer Pack", "sensorjammerpack");
		WH::AddItem("Turret", "turret");
		WH::AddItem("Chameleon Pack", "ammopack");
		WH::AddItem("Cloaking Device", "sensorjammerpack");
		WH::AddItem("Laptop", "ammostation");
		WH::AddItem("Ghost Pack", "sensorjammerpack");
		WH::AddItem("Phase Shifter", "shieldpack");
		WH::AddItem("Troll Pack", "ammopack");
		WH::AddItem("StealthShield Pack", "shieldpack");
		WH::AddItem("Suicide DetPack", "motionsensor");
		WH::AddItem("Tank Pack", "turret");
		WH::AddItem("True Sight Pack", "sensorjammerpack");
		WH::AddItem("Survey Droid", "camera");
		WH::AddItem("Probe Droid", "camera");
		WH::AddItem("Suicide Droid", "camera");
	}
	else if($ServerMod == "Annihilation Duel")
	{
		//base mod
		WH::AddItem("Plasma Gun", "plasma", "Plasma Bolt");
		WH::AddItem("Blaster","blaster");
		WH::AddItem("ChainGun", "chaingun", "Bullet");
		WH::AddItem("Disc Launcher", "disc", "Disc");
		WH::AddItem("Elf Gun", "elf");
		WH::AddItem("Grenade Launcher", "grenade", "Grenade Ammo");
		WH::AddItem("Laser Rifle", "sniper");
		WH::AddItem("Mortar", "mortar", "Mortar Ammo");
	
		WH::AddItem("Repair Gun", "repairpack");
		WH::AddItem("Inventory Station", "inventory");
		WH::AddItem("Ammo Station", "ammostation");
		WH::AddItem("Ammo Pack", "ammopack");
		WH::AddItem("Camera" ,"camera");
		WH::AddItem("Energy Pack" ,"energypack");
		WH::AddItem("Motion Sensor","motionsensor");
		WH::AddItem("Pulse Sensor", "pulse");
		WH::AddItem("Repair Pack" ,"repairpack");
		WH::AddItem("Shield Pack" ,"shieldpack");
		WH::AddItem("Sensor Jammer Pack", "sensorjammerpack");
		WH::AddItem("Sensor Jammer", "sensorjammer");
		WH::AddItem("Turret", "turret");
	}
	else if($ServerMod == "base")
	{
		//base mod
		WH::AddItem("Plasma Gun", "plasma", "Plasma Bolt");
		WH::AddItem("Blaster","blaster");
		WH::AddItem("ChainGun", "chaingun", "Bullet");
		WH::AddItem("Disc Launcher", "disc", "Disc");
		WH::AddItem("Elf Gun", "elf");
		WH::AddItem("Grenade Launcher", "grenade", "Grenade Ammo");
		WH::AddItem("Laser Rifle", "sniper");
		WH::AddItem("Mortar", "mortar", "Mortar Ammo");
	
		WH::AddItem("Repair Gun", "repairpack");
		WH::AddItem("Inventory Station", "inventory");
		WH::AddItem("Ammo Station", "ammostation");
		WH::AddItem("Ammo Pack", "ammopack");
		WH::AddItem("Camera" ,"camera");
		WH::AddItem("Energy Pack" ,"energypack");
		WH::AddItem("Motion Sensor","motionsensor");
		WH::AddItem("Pulse Sensor", "pulse");
		WH::AddItem("Repair Pack" ,"repairpack");
		WH::AddItem("Shield Pack" ,"shieldpack");
		WH::AddItem("Sensor Jammer Pack", "sensorjammerpack");
		WH::AddItem("Sensor Jammer", "sensorjammer");
		WH::AddItem("Turret", "turret");
	}
	else if($ServerVersion == "1.40" && $ServerMod == "")
	{
		//base mod
		WH::AddItem("Plasma Gun", "plasma", "Plasma Bolt");
		WH::AddItem("Blaster","blaster");
		WH::AddItem("ChainGun", "chaingun", "Bullet");
		WH::AddItem("Disc Launcher", "disc", "Disc");
		WH::AddItem("Elf Gun", "elf");
		WH::AddItem("Grenade Launcher", "grenade", "Grenade Ammo");
		WH::AddItem("Laser Rifle", "sniper");
		WH::AddItem("Mortar", "mortar", "Mortar Ammo");
	
		WH::AddItem("Repair Gun", "repairpack");
		WH::AddItem("Inventory Station", "inventory");
		WH::AddItem("Ammo Station", "ammostation");
		WH::AddItem("Ammo Pack", "ammopack");
		WH::AddItem("Camera" ,"camera");
		WH::AddItem("Energy Pack" ,"energypack");
		WH::AddItem("Motion Sensor","motionsensor");
		WH::AddItem("Pulse Sensor", "pulse");
		WH::AddItem("Repair Pack" ,"repairpack");
		WH::AddItem("Shield Pack" ,"shieldpack");
		WH::AddItem("Sensor Jammer Pack", "sensorjammerpack");
		WH::AddItem("Sensor Jammer", "sensorjammer");
		WH::AddItem("Turret", "turret");
	}
	else if($ServerMod == "EliteRenegades")
	{
		//base mod
		WH::AddItem("Plasma Gun", "plasma", "Plasma Bolt");
		WH::AddItem("Blaster","blaster");
		WH::AddItem("ChainGun", "chaingun", "Bullet");
		WH::AddItem("Disc Launcher", "disc", "Disc");
		WH::AddItem("Elf Gun", "elf");
		WH::AddItem("Grenade Launcher", "grenade", "Grenade Ammo");
		WH::AddItem("Laser Rifle", "sniper");
		WH::AddItem("Mortar", "mortar", "Mortar Ammo");
	
		WH::AddItem("Repair Gun", "repairpack");
		WH::AddItem("Inventory Station", "inventory");
		WH::AddItem("Ammo Station", "ammostation");
		WH::AddItem("Ammo Pack", "ammopack");
		WH::AddItem("Camera" ,"camera");
		WH::AddItem("Energy Pack" ,"energypack");
		WH::AddItem("Motion Sensor","motionsensor");
		WH::AddItem("Pulse Sensor", "pulse");
		WH::AddItem("Repair Pack" ,"repairpack");
		WH::AddItem("Shield Pack" ,"shieldpack");
		WH::AddItem("Sensor Jammer Pack", "sensorjammerpack");
		WH::AddItem("Sensor Jammer", "sensorjammer");
		WH::AddItem("Turret", "turret");
	}
	else if($ServerMod == "EliteRenegades base")
	{
		//base mod
		WH::AddItem("Plasma Gun", "plasma", "Plasma Bolt");
		WH::AddItem("Blaster","blaster");
		WH::AddItem("ChainGun", "chaingun", "Bullet");
		WH::AddItem("Disc Launcher", "disc", "Disc");
		WH::AddItem("Elf Gun", "elf");
		WH::AddItem("Grenade Launcher", "grenade", "Grenade Ammo");
		WH::AddItem("Laser Rifle", "sniper");
		WH::AddItem("Mortar", "mortar", "Mortar Ammo");
	
		WH::AddItem("Repair Gun", "repairpack");
		WH::AddItem("Inventory Station", "inventory");
		WH::AddItem("Ammo Station", "ammostation");
		WH::AddItem("Ammo Pack", "ammopack");
		WH::AddItem("Camera" ,"camera");
		WH::AddItem("Energy Pack" ,"energypack");
		WH::AddItem("Motion Sensor","motionsensor");
		WH::AddItem("Pulse Sensor", "pulse");
		WH::AddItem("Repair Pack" ,"repairpack");
		WH::AddItem("Shield Pack" ,"shieldpack");
		WH::AddItem("Sensor Jammer Pack", "sensorjammerpack");
		WH::AddItem("Sensor Jammer", "sensorjammer");
		WH::AddItem("Turret", "turret");
	}
	else if($ServerMod == "Renegades") //some renegades player is going to have to update this one -DaJ4ck3L
	{
		//base mod
		WH::AddItem("Plasma Gun", "plasma", "PlasmaBolt");
		WH::AddItem("Blaster","blaster");
		WH::AddItem("ChainGun", "chaingun", "Bullet");
		WH::AddItem("Disc Launcher", "disc", "Disc");
		WH::AddItem("Elf Gun", "elf");
		WH::AddItem("Grenade Launcher", "grenade", "Grenade Ammo");
		WH::AddItem("Laser Rifle", "sniper");
		WH::AddItem("Mortar", "mortar", "Mortar Ammo");
	
		WH::AddItem("Repair Gun", "repairpack");
		WH::AddItem("Inventory Station", "inventory");
		WH::AddItem("Ammo Station", "ammostation");
		WH::AddItem("Ammo Pack", "ammopack");
		WH::AddItem("Camera" ,"camera");
		WH::AddItem("Energy Pack" ,"energypack");
		WH::AddItem("Motion Sensor","motionsensor");
		WH::AddItem("Pulse Sensor", "pulse");
		WH::AddItem("Repair Pack" ,"repairpack");
		WH::AddItem("Shield Pack" ,"shieldpack");
		WH::AddItem("Sensor Jammer Pack", "sensorjammerpack");
		WH::AddItem("Sensor Jammer", "sensorjammer");
		WH::AddItem("Turret", "turret");
		
		//Renegades Classic Stuff, mostly I just made shit look like an ammopack
		WH::AddItem("Hyper Blaster","plasma");
		WH::AddItem("Rocket Launcher","mortar","Rockets");
		WH::AddItem("Sniper Rifle","sniper","Sniper Bullet");
		WH::AddItem("Dart Rifle","sniper","Poison Dart");
		WH::AddItem("Magnum","blaster","Magnum Bullets");
		WH::AddItem("Shockwave Cannon","elf");
		WH::AddItem("Railgun","sniper","Railgun Bolt");
		WH::AddItem("Vulcan","chaingun","Vulcan Bullet");
		WH::AddItem("Flame Thrower","grenade");
		WH::AddItem("Ion Rifle","grenade");
		WH::AddItem("Omega Cannon","elf");
		WH::AddItem("Thunderbolt","elf");
		WH::AddItem("Engineer Repair-Gun","target");
		WH::AddItem("Cloaking Device","sensorjammerpack");
		WH::AddItem("StealthShield Pack","shieldpack");
		WH::AddItem("Regeneration Pack","repairpack");
		WH::AddItem("Lightning Pack","energypack");
		WH::AddItem("Suicide DetPack","ammopack");
		WH::AddItem("Command Station","inventory");
		WH::AddItem("Ion Turret","turret");
		WH::AddItem("Laser Turret","camera");
		WH::AddItem("Shock Turret","camera");
		WH::AddItem("Mortar Turret","turret");
		WH::AddItem("Plasma Turret","turret");
		WH::AddItem("Vulcan Turret","turret");
		WH::AddItem("Rail Turret","turret");
		WH::AddItem("Missile Turret","turret");
		WH::AddItem("Force Field","ammopack");
		WH::AddItem("Large Force Field","ammopack");
		WH::AddItem("Blast Wall","ammopack");
		WH::AddItem("Hologram","ammopack");
		WH::AddItem("Mechanical Tree","ammopack");
		WH::AddItem("Springboard","ammopack");
		WH::AddItem("Deployable Platform","ammopack");
		WH::AddItem("Teleport Pad","ammopack");
		WH::AddItem("Interceptor Pack","ammopack");
		WH::AddItem("StealthHPC Pack","ammopack");
	}

	//I don't give a shit about mods, this is a bitch, someone else can write more here

	//Sort the weapons list by ID so they are in order
	WH::Sort();

	//Add to GUI
	WH::Create();
}
