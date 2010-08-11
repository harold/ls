;; Binding test
(define box (fn (contents title) (. 'System.Windows.Forms.MessageBox 'Show contents title (. 'System.Windows.Forms.MessageBoxButtons 'OKCancel) (. 'System.Windows.Forms.MessageBoxIcon 'Error))))

;; Helpers
(define file-text (fn (path) (. 'System.IO.File 'ReadAllText path)))
(define read (fn (str) (. 'ls.Reader 'Read str)))
(define eval (fn (str) (. 'ls.Evaluator 'Eval str *env*)))

;; Math
(define pi (. 'System.Math 'PI))
(define tau (* pi 2))
(define sin (fn (x) (. 'System.Math 'Sin x)))
(define cos (fn (x) (. 'System.Math 'Cos x)))
(define tan (fn (x) (. 'System.Math 'Tan x)))
(define rad (fn (x) (* pi (/ x 180.0))))
(define deg (fn (x) (* x (/ 180.0 pi))))

;; Looping
;; example: (dotimes 3 '(print "yo"))
(define dotimes (fn (i expr)
  (if (= i 0)
    'done
    (begin
      (eval expr)
      (dotimes (- i 1) expr)))))

;; Sqrt (with apologies to Abelson, Sussman, et. al.)
(define sqrt-iter (fn (guess x)
  (if (good-enough? guess x)
      guess
      (sqrt-iter (improve guess x) x))))

(define improve (fn (guess x)
  (average guess (/ x guess))))

(define average (fn (x y)
  (/ (+ x y) 2.0)))

(define square (fn (x) (* x x)))

(define abs (fn (x) (if (< x 0) (- 0 x) x)))

(define good-enough? (fn (guess x)
  (< (abs (- (square guess) x)) 0.001)))

(define sqrt (fn (x) (sqrt-iter 1.0 x)))

;; Logic
(define not (fn (b) (if b false true)))
(define when   (macro (p e) `(if ,p ,e null)))
(define unless (macro (p e) `(if ,p null ,e)))

;; Lists
(define count  (fn (l) (. l 'Count)))
(define cons   (fn (a b) (if (not b) `(,a) (begin (. b 'Insert 0 a) b))))
(define first  (fn (l) (. l 'get_Item 0)))
(define rest   (fn (l) (. l 'GetRange 1 (- (count l) 1))))
(define second (fn (l) (first (rest l))))
(define take   (fn (n l) (. l 'GetRange 0 n)))
(define drop   (fn (n l) (. l 'GetRange n (- (count l) n))))
(define map    (fn (f l) (if (= (count l) 0) null (cons (f (first l)) (map f (rest l))))))

;; Hashtables
(define assoc (fn (h k v) (begin (. h 'set_Item k v) h)))

;; Let
;; example: (let-one a 1 (+ a 2)) ;==> 3
;; example: (let (a 1 b (+ 1 a)) (+ a b)) ;==> 3
(define let-one (macro (name value expression) `(`(fn `(,name) ,expression) ,value)))
(define let (macro (l e) `(let-one ,(first l) ,(second l) ,(if (= (count l) 2) e `(let ,(drop 2 l) ,e)))))
