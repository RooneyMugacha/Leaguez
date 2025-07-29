using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Leaguez.Data;
using Leaguez.Models;

namespace Leaguez.Controllers
{
    public class LeaguesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LeaguesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Leagues
        public async Task<IActionResult> Index()
        {
            var leagues = await _context.Leagues.OrderByDescending(l => l.CreatedAt).ToListAsync();
            return View(leagues); // Passes the list of leagues to the View
        }

        // GET: /Leagues/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var league = await _context.Leagues
                .Include(l => l.Players)
                .Include(l => l.Matches).ThenInclude(m => m.Player1)
                .Include(l => l.Matches).ThenInclude(m => m.Player2)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (league == null) return NotFound();

            // Sort players for the league table
            league.Players = league.Players.OrderByDescending(p => p.Points)
                                           .ThenByDescending(p => p.GoalsFor - p.GoalsAgainst)
                                           .ThenByDescending(p => p.GoalsFor)
                                           .ToList();

            return View(league);
        }

        // GET: /Leagues/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Leagues/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string leagueName, List<string> players)
        {
            if (string.IsNullOrWhiteSpace(leagueName))
            {
                // Add error message to display in the view
                ModelState.AddModelError("", "League name and at least 2 players are required.");
                return View();
            }

            // Create League
            var league = new League { Name = leagueName };
            _context.Leagues.Add(league);
            await _context.SaveChangesAsync(); // Save to get the new League ID

            // Create Players
            var playerEntities = new List<Player>();
            foreach (var playerName in players.Where(p => !string.IsNullOrWhiteSpace(p)))
            {
                var player = new Player { Name = playerName, LeagueId = league.Id };
                playerEntities.Add(player);
                _context.Players.Add(player);
            }
            await _context.SaveChangesAsync(); // Save to get Player IDs

            // Create Fixtures
            for (int i = 0; i < playerEntities.Count; i++)
            {
                for (int j = i + 1; j < playerEntities.Count; j++)
                {
                    var match = new Match
                    {
                        Player1Id = playerEntities[i].Id,
                        Player2Id = playerEntities[j].Id,
                        LeagueId = league.Id,
                        IsPlayed = false
                    };
                    _context.Matches.Add(match);
                }
            }
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = league.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordResult(int matchId, int score1, int score2)
        {
            var match = await _context.Matches
                .Include(m => m.Player1)
                .Include(m => m.Player2)
                .FirstOrDefaultAsync(m => m.Id == matchId);

            if (match == null) return NotFound();

            // --- Step 1: Revert old stats if the match was already played (for editing results) ---
            if (match.IsPlayed)
            {
                match.Player1.Played--;
                match.Player2.Played--;
                match.Player1.GoalsFor -= match.Score1.Value;
                match.Player1.GoalsAgainst -= match.Score2.Value;
                match.Player2.GoalsFor -= match.Score2.Value;
                match.Player2.GoalsAgainst -= match.Score1.Value;

                if (match.Score1 > match.Score2) // Player 1 won previously
                {
                    match.Player1.Wins--;
                    match.Player1.Points -= 3;
                    match.Player2.Losses--;
                }
                else if (match.Score2 > match.Score1) // Player 2 won previously
                {
                    match.Player2.Wins--;
                    match.Player2.Points -= 3;
                    match.Player1.Losses--;
                }
                else // It was a draw
                {
                    match.Player1.Draws--;
                    match.Player1.Points += 1;
                    match.Player2.Draws--;
                    match.Player2.Points += 1;
                }
            }

            // --- Step 2: Apply new stats based on the submitted scores ---
            match.Score1 = score1;
            match.Score2 = score2;
            match.IsPlayed = true;

            match.Player1.Played++;
            match.Player2.Played++;
            match.Player1.GoalsFor += score1;
            match.Player1.GoalsAgainst += score2;
            match.Player2.GoalsFor += score2;
            match.Player2.GoalsAgainst += score1;

            if (score1 > score2) // Player 1 wins
            {
                match.Player1.Wins++;
                match.Player1.Points += 3;
                match.Player2.Losses++;
            }
            else if (score2 > score1) // Player 2 wins
            {
                match.Player2.Wins++;
                match.Player2.Points += 3;
                match.Player1.Losses++;
            }
            else // Draw
            {
                match.Player1.Draws++;
                match.Player1.Points += 1;
                match.Player2.Draws++;
                match.Player2.Points += 1;
            }

            await _context.SaveChangesAsync();

            // Redirect back to the details page for the current league
            return RedirectToAction("Details", new { id = match.LeagueId });
        }
    }

}