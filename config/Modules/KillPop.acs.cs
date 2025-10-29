function KillPop( %killer, %victim, %damage ) {
	if ( %killer != getManagerId() )
		return;
	
	remoteEP("<JC><F2>You <F2>KILLED <F2>" ~ String::escapeFormatting( Client::getName( %victim ) ) ~ " :: <F2>" ~ %damage, 3, 1,1, 14, 400);
	localSound(gotcha);
}

function TeamKillPop( %killer, %victim, %damage ) {
	if ( %killer != getManagerId() )
		return;
	remoteEP("<JC><F2>You <F2>TKed <F2>" ~ String::escapeFormatting( Client::getName( %victim ) ), 3, 1, 1, 13, 200);
	localSound(nelson);
}

Event::Attach( eventClientKilled, KillPop );
Event::Attach( eventClientTeamKilled, TeamKillPop );