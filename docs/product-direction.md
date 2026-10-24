# Product direction

The user's requested target is 1,000 progressively larger independent products, with real deployment and ongoing discussion between agents. The commercial aspiration is substantial revenue and millions of users; neither has been observed. Shipping alone is not evidence of demand.

## Progression

1. Standalone games with replayable systems and clear rules.
2. Creator tools whose outputs can be shared and reused.
3. Persistent social products with real contributions and abuse controls.
4. Larger communities, reusable user creations, collaborative editing, multiplayer systems, and discovery based on actual activity.

Every counted project needs a distinct user problem or entertainment mechanic and a working implementation. Additional maps, colors, prompts, themes, or datasets alone do not qualify as new sites. Projects live on independent paths to fit hosting quotas, without a common shell restricting their interface or behavior.

## First discussion

- The games agent recommends replayable seeded systems and local records as reasons to return.
- The creative agent proposes user-made escape adventures shared by URL as an invitation loop.
- The social agent proposes one-paragraph contributions to a branching story as an easy reader-to-author transition.

These are product hypotheses. Actual sharing, return use, and creator activity must be measured before prioritization is presented as proven.

## Current delivery architecture

Source is public in https://github.com/muhammetozeski/BinSite. GitHub Actions builds the directory and uses Cloudflare's official Wrangler action to deploy `public` plus Pages Functions. This follows the already operating publication pattern in the user's AllRepos project. Production secrets live in GitHub Actions secrets, not source. Shared social state uses a Cloudflare D1 database; standalone products remain usable without the database.

