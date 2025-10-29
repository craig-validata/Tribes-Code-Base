function Auto::Health( ) {

	if ( $Health < 70 && $Health > 0 )
		use("Repair Kit");
	schedule::add("Auto::Health();", 1);
}

Event::Attach(eventConnected, Auto::Health);